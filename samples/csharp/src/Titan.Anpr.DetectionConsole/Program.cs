using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Titan.Anpr.NativeInterop;

namespace Titan.Anpr.DetectionConsole;

/// <summary>
/// Console sample: run Titan-ANPR on one image or all images in a folder, print metrics, and save overlay previews.
/// Uses the unified API multi-result mode (`TitanANPR_Detect` list output).
/// </summary>
internal static class Program
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff"
    };

    private const int MaxResults = 32;
    private static bool _winFormsInitialized;

    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Optional: folder that contains Titan-ANPR.dll (absolute or relative to the process working directory).
        string? nativeBinDirectory = @"C:\repos\SR_ANPR_SDK\bin";
        NativeDll.BinDirectoryOverride = nativeBinDirectory;

        if (args.Length == 0 || HasHelpSwitch(args))
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        var options = ParseArgs(args);
        if (options.InputPath == null)
        {
            Console.Error.WriteLine("Missing input path (image file or directory).");
            PrintUsage();
            return 1;
        }

        try
        {
            NativeDll.EnsureLoaded();
        }
        catch (DllNotFoundException ex)
        {
            Console.Error.WriteLine("Failed to load native library: " + ex.Message);
            return 1;
        }

        var initRc = TitanAnprNative.TitanANPR_Init(out var handle);
        if (initRc != 0 || handle == IntPtr.Zero)
        {
            Console.Error.WriteLine("TitanANPR_Init failed. Return code: " + initRc + " (invalid license is often -10).");
            return 1;
        }

        try
        {
            if (File.Exists(options.InputPath))
                return ProcessSingleFile(handle, options.InputPath, options, waitAfterStep: true) ? 0 : 1;

            if (Directory.Exists(options.InputPath))
                return ProcessDirectory(handle, options.InputPath, options);

            Console.Error.WriteLine("Path is not a file or directory: " + options.InputPath);
            return 1;
        }
        finally
        {
            TitanAnprNative.TitanANPR_Dispose(handle);
        }
    }

    private static bool HasHelpSwitch(string[] args) =>
        args.Any(a => a is "-h" or "-?" or "--help" or "/?" or "/help");

    private static void PrintUsage()
    {
        Console.WriteLine("""
            Titan-ANPR Detection Console (sample)

            Usage:
              Titan.Anpr.DetectionConsole.exe <image|directory> [--out <folder>] [--no-open]

            Arguments:
              image|directory   Single image file, or a folder whose image files are processed in name order.
              --out <folder>    Optional. Save overlay PNGs here (default: next to each source file).
              --no-open         Do not show the preview window; wait on the console instead (press a key between images).

            Images are processed one at a time. After each image you must press a key in the preview window (or close it)
            before the next one runs, so only one window is open. With --no-open, the same pacing uses the console.

            For each image the sample prints all returned plates from TitanANPR_Detect,
            with text, confidence and total engine time for the full Detect call.
            Overlay PNGs show one quadrilateral/box per returned result.

            Native DLL resolution matches other samples (see Titan.Anpr.NativeInterop / Program.Main nativeBinDirectory).
            """);
    }

    private static Options ParseArgs(string[] args)
    {
        string? input = null;
        string? outDir = null;
        var noOpen = false;

        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];
            if (a.StartsWith('-'))
            {
                if (a is "--no-open")
                    noOpen = true;
                else if (a is "--out" && i + 1 < args.Length)
                    outDir = args[++i];
                continue;
            }

            if (input == null)
                input = a;
        }

        return new Options(input, outDir, noOpen);
    }

    private sealed class Options(string? inputPath, string? overlayOutputDirectory, bool noOpen)
    {
        public string? InputPath { get; } = inputPath;
        public string? OverlayOutputDirectory { get; } = overlayOutputDirectory;
        public bool NoOpen { get; } = noOpen;
        public string? LastOverlayPath { get; set; }
    }

    private static int ProcessDirectory(IntPtr handle, string directory, Options options)
    {
        var files = Directory.EnumerateFiles(directory)
            .Where(f => ImageExtensions.Contains(Path.GetExtension(f)))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
        {
            Console.WriteLine("No supported images found in: " + directory);
            return 0;
        }

        var anyFail = false;
        foreach (var file in files)
        {
            if (!ProcessSingleFile(handle, file, options, waitAfterStep: true))
                anyFail = true;
        }

        return anyFail ? 1 : 0;
    }

    private static bool ProcessSingleFile(IntPtr handle, string imagePath, Options options, bool waitAfterStep)
    {
        void PromptBeforeNext()
        {
            if (waitAfterStep)
                WaitForUserBeforeNextImage(options);
        }

        Console.WriteLine();
        Console.WriteLine("File: " + imagePath);

        Bitmap? rgb = null;
        try
        {
            rgb = OverlayRenderer.LoadAsRgb24(imagePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("  Failed to load image: " + ex.Message);
            PromptBeforeNext();
            return false;
        }

        using (rgb)
        {
            var rect = new Rectangle(0, 0, rgb.Width, rgb.Height);
            BitmapData data;
            try
            {
                data = rgb.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("  LockBits failed: " + ex.Message);
                PromptBeforeNext();
                return false;
            }

            var results = new TitanAnprNative.TitanAnprResult[MaxResults];
            var sw = Stopwatch.StartNew();
            int detectRc;
            int returnedCount;
            try
            {
                detectRc = TitanAnprNative.TitanANPR_Detect(
                    handle,
                    data.Scan0,
                    rgb.Width,
                    rgb.Height,
                    data.Stride,
                    results,
                    MaxResults,
                    out returnedCount);
            }
            finally
            {
                rgb.UnlockBits(data);
            }
            sw.Stop();

            if (detectRc != 0)
            {
                Console.WriteLine("  TitanANPR_Detect return code: " + detectRc);
                PromptBeforeNext();
                return false;
            }

            if (returnedCount < 0)
                returnedCount = 0;
            if (returnedCount > MaxResults)
                returnedCount = MaxResults;

            var ms = sw.Elapsed.TotalMilliseconds.ToString("0.##", CultureInfo.InvariantCulture);
            Console.WriteLine("  Returned plates:  " + returnedCount);
            Console.WriteLine("  Engine time:      " + ms + " ms");

            for (var i = 0; i < returnedCount; i++)
            {
                var r = results[i];
                var plate = string.IsNullOrWhiteSpace(r.plate_text) ? "(none)" : r.plate_text.Trim('\0', ' ');
                var conf = r.total_confidence.ToString("0.###", CultureInfo.InvariantCulture);
                Console.WriteLine($"  [{i + 1}] Plate: {plate} | Total confidence: {conf}");
            }

            using var overlay = (Bitmap)rgb.Clone();
            using (var g = Graphics.FromImage(overlay))
            {
                if (returnedCount > 0)
                    OverlayRenderer.DrawPlateOverlays(g, results.Take(returnedCount).ToArray());
                else
                    OverlayRenderer.DrawNoPlateBanner(g, overlay.Width, overlay.Height);
            }

            var outPath = BuildOverlayPath(imagePath, options.OverlayOutputDirectory);
            try
            {
                OverlayRenderer.SavePng(overlay, outPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("  Failed to save overlay: " + ex.Message);
                PromptBeforeNext();
                return false;
            }

            Console.WriteLine("  Overlay:          " + outPath);
            options.LastOverlayPath = outPath;

            PromptBeforeNext();
            return true;
        }
    }

    private static void WaitForUserBeforeNextImage(Options options)
    {
        var path = options.LastOverlayPath;
        if (!options.NoOpen && !string.IsNullOrEmpty(path) && File.Exists(path))
        {
            EnsureWinFormsInitialized();
            Application.Run(new OverlayPreviewForm(path));
            return;
        }

        Console.WriteLine();
        Console.WriteLine("  Press any key for the next image (or to finish)…");
        Console.ReadKey(intercept: true);
        Console.WriteLine();
    }

    private static void EnsureWinFormsInitialized()
    {
        if (_winFormsInitialized)
            return;
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        _winFormsInitialized = true;
    }

    private static string BuildOverlayPath(string imagePath, string? outputDirectory)
    {
        var name = Path.GetFileNameWithoutExtension(imagePath) + "_titan_overlay.png";
        if (string.IsNullOrWhiteSpace(outputDirectory))
            return Path.Combine(Path.GetDirectoryName(imagePath) ?? ".", name);
        return Path.Combine(outputDirectory, name);
    }
}
