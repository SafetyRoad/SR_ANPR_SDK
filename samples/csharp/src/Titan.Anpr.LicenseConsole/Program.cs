using System.Text;
using Titan.Anpr.NativeInterop;

namespace Titan.Anpr.LicenseConsole;

/// <summary>
/// Console entry point for Titan-ANPR license management sample.
/// Demonstrates hardware ID retrieval, activation, deactivation, validation, and status inspection.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Optional: folder that contains Titan-ANPR.dll (absolute or relative to the process working directory).
        // Leave null to use TITAN_ANPR_NATIVE_DIR, then repository layout discovery, then the app directory.
        string? nativeBinDirectory = @"C:\repos\SR_ANPR_SDK\bin";
        // Example: nativeBinDirectory = @"C:\repos\SR_ANPR_SDK\bin";
        NativeDll.BinDirectoryOverride = nativeBinDirectory;

        if (args.Length > 0 && IsHelpSwitch(args[0]))
        {
            PrintUsage();
            return 0;
        }

        PrintUsage();
        Console.WriteLine();

        try
        {
            NativeDll.EnsureLoaded();
        }
        catch (DllNotFoundException ex)
        {
            Console.Error.WriteLine("Failed to load native library: " + ex.Message);
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options: (1) Set nativeBinDirectory in Program.Main");
            Console.Error.WriteLine("         (2) Build the native SDK under bin/Titan-ANPR/Release/x64/bin relative to the repo root");
            Console.Error.WriteLine("         (3) Copy Titan-ANPR.dll and dependencies next to this executable");
            Console.Error.WriteLine("         (4) Set TITAN_ANPR_NATIVE_DIR=<folder containing Titan-ANPR.dll>");
            return 1;
        }

        if (args.Length > 0)
            return RunCommandLine(args);

        return RunInteractive();
    }

    private static bool IsHelpSwitch(string a) =>
        a is "-h" or "-?" or "--help" or "/?" or "/help";

    private static void PrintUsage()
    {
        Console.WriteLine("""
            Titan-ANPR License Console (sample)

            In code:
              Program.Main — set nativeBinDirectory to the folder containing Titan-ANPR.dll, or null for automatic resolution.

            Environment:
              TITAN_ANPR_NATIVE_DIR   Optional. Directory containing Titan-ANPR.dll and native dependencies.

            Native search (when code path and env are unset):
              Walks up from the app folder for bin/Titan-ANPR/Release/x64/bin or .../Debug/x64/bin, then the app directory.

            Interactive mode (no arguments):
              Run without arguments for a menu-driven session.

            Commands:
              info              Print current license info (status, dates, customer).
              hwid              Print this machine's hardware ID (for vendor activation).
              validate          Run the same validation TitanANPR_Init uses.
              activate <key>    Activate using the provided license key.
              deactivate        Clear local activation (may return to trial if applicable).

            Licensing:
              Licenses are activated by sending your hardware ID (use the "hwid" command) to licenses@safetyroad.es.
              You will receive a license key to use with "activate".

            Examples:
              Titan.Anpr.LicenseConsole.exe info
              Titan.Anpr.LicenseConsole.exe hwid
              Titan.Anpr.LicenseConsole.exe activate "YOUR-LICENSE-KEY"
              Titan.Anpr.LicenseConsole.exe deactivate
            """);
    }

    private static int RunCommandLine(string[] args)
    {
        var cmd = args[0].ToLowerInvariant();
        return cmd switch
        {
            "info" => CmdInfo(),
            "hwid" => CmdHwid(),
            "validate" => CmdValidate(),
            "activate" => args.Length < 2 ? Fail("Missing license key. Usage: activate <key>") : CmdActivate(args[1]),
            "deactivate" => CmdDeactivate(),
            _ => Fail($"Unknown command: {args[0]}. Use --help.")
        };
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 2;
    }

    private static int RunInteractive()
    {
        Console.WriteLine("Interactive mode — type a number or 'q' to quit.");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("1) Show license info");
            Console.WriteLine("2) Show hardware ID");
            Console.WriteLine("3) Validate license (runtime check)");
            Console.WriteLine("4) Activate with license key");
            Console.WriteLine("5) Deactivate / clear local license");
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var token = line.Trim();
            if (token.Equals("q", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("exit", StringComparison.OrdinalIgnoreCase))
                return 0;

            switch (token)
            {
                case "1":
                    CmdInfo();
                    break;
                case "2":
                    CmdHwid();
                    break;
                case "3":
                    CmdValidate();
                    break;
                case "4":
                    Console.Write("License key: ");
                    var key = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(key))
                        CmdActivate(key.Trim());
                    break;
                case "5":
                    CmdDeactivate();
                    break;
                default:
                    Console.WriteLine("Unknown option. Choose 1–5 or q.");
                    break;
            }

            Console.WriteLine();
        }
    }

    private static int CmdInfo()
    {
        var info = default(TitanLicenseNative.TitanLicenseInfo);
        var rc = TitanLicenseNative.TitanLicense_GetInfo(ref info);
        Console.WriteLine("TitanLicense_GetInfo return code: " + FormatReturnCode(rc));
        Console.WriteLine("  status:          " + FormatStatus(info.status));
        Console.WriteLine("  days_remaining:  " + info.days_remaining);
        Console.WriteLine("  days_total:      " + info.days_total);
        Console.WriteLine("  customer_name:   " + (info.customer_name ?? ""));
        Console.WriteLine("  license_key:     " + MaskKey(info.license_key));
        Console.WriteLine("  hardware_id:     " + (info.hardware_id ?? ""));
        Console.WriteLine("  expiry_date:     " + (info.expiry_date ?? ""));
        return rc == TitanLicenseNative.Ok ? 0 : 1;
    }

    private static int CmdHwid()
    {
        // Buffer size must match vendor expectations; 256 bytes is safe for null-terminated ASCII.
        var buffer = new byte[256];
        var rc = TitanLicenseNative.TitanLicense_GetHardwareId(buffer, buffer.Length);
        Console.WriteLine("TitanLicense_GetHardwareId return code: " + FormatReturnCode(rc));
        if (rc != TitanLicenseNative.Ok)
            return 1;

        var len = Array.IndexOf(buffer, (byte)0);
        if (len < 0)
            len = buffer.Length;
        var id = Encoding.ASCII.GetString(buffer, 0, len);
        Console.WriteLine("Hardware ID: " + id);
        return 0;
    }

    private static int CmdValidate()
    {
        var rc = TitanLicenseNative.TitanLicense_Validate();
        Console.WriteLine("TitanLicense_Validate return code: " + FormatReturnCode(rc));
        return rc == TitanLicenseNative.Ok ? 0 : 1;
    }

    private static int CmdActivate(string licenseKey)
    {
        var rc = TitanLicenseNative.TitanLicense_Activate(licenseKey);
        Console.WriteLine("TitanLicense_Activate return code: " + FormatReturnCode(rc));
        return rc == TitanLicenseNative.Ok ? 0 : 1;
    }

    private static int CmdDeactivate()
    {
        var rc = TitanLicenseNative.TitanLicense_Deactivate();
        Console.WriteLine("TitanLicense_Deactivate return code: " + FormatReturnCode(rc));
        return rc == TitanLicenseNative.Ok ? 0 : 1;
    }

    private static string FormatReturnCode(int rc) =>
        rc + " (" + DescribeLicenseError(rc) + ")";

    private static string DescribeLicenseError(int rc) => rc switch
    {
        TitanLicenseNative.Ok => "OK",
        TitanLicenseNative.ErrInvalidKey => "ERR_INVALID_KEY",
        TitanLicenseNative.ErrExpired => "ERR_EXPIRED",
        TitanLicenseNative.ErrHwMismatch => "ERR_HW_MISMATCH",
        TitanLicenseNative.ErrTampered => "ERR_TAMPERED",
        TitanLicenseNative.ErrIo => "ERR_IO",
        TitanLicenseNative.ErrInternal => "ERR_INTERNAL",
        TitanLicenseNative.ErrClock => "ERR_CLOCK",
        _ => "UNKNOWN"
    };

    private static string FormatStatus(int status) => status switch
    {
        TitanLicenseNative.StatusTrial => "TRIAL",
        TitanLicenseNative.StatusActive => "ACTIVE",
        TitanLicenseNative.StatusExpired => "EXPIRED",
        TitanLicenseNative.StatusInvalid => "INVALID",
        TitanLicenseNative.StatusRevoked => "REVOKED",
        _ => "UNKNOWN(" + status + ")"
    };

    private static string MaskKey(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return "";
        if (key.Length <= 8)
            return "****";
        return key[..4] + "..." + key[^4..];
    }
}
