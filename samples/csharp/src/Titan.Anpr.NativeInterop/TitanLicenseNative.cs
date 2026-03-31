using System.Runtime.InteropServices;

namespace Titan.Anpr.NativeInterop;

/// <summary>
/// P/Invoke bindings for the Titan-ANPR license API (<c>titan_license.h</c>).
/// </summary>
public static class TitanLicenseNative
{
    public const int StatusTrial = 0;
    public const int StatusActive = 1;
    public const int StatusExpired = 2;
    public const int StatusInvalid = 3;
    public const int StatusRevoked = 4;

    public const int Ok = 0;
    public const int ErrInvalidKey = -1;
    public const int ErrExpired = -2;
    public const int ErrHwMismatch = -3;
    public const int ErrTampered = -4;
    public const int ErrIo = -5;
    public const int ErrInternal = -6;
    public const int ErrClock = -7;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TitanLicenseInfo
    {
        public int status;
        public int days_remaining;
        public int days_total;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string customer_name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string license_key;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string hardware_id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string expiry_date;
    }

    [DllImport(NativeDll.DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true, EntryPoint = "TitanLicense_GetInfo")]
    public static extern int TitanLicense_GetInfo(ref TitanLicenseInfo out_info);

    [DllImport(NativeDll.DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true, EntryPoint = "TitanLicense_GetHardwareId")]
    public static extern int TitanLicense_GetHardwareId([Out] byte[] buffer, int buffer_size);

    [DllImport(NativeDll.DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true, EntryPoint = "TitanLicense_Activate")]
    public static extern int TitanLicense_Activate([MarshalAs(UnmanagedType.LPStr)] string license_key);

    [DllImport(NativeDll.DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "TitanLicense_Deactivate")]
    public static extern int TitanLicense_Deactivate();

    [DllImport(NativeDll.DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "TitanLicense_Validate")]
    public static extern int TitanLicense_Validate();
}
