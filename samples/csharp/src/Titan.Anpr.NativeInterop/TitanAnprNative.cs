using System.Runtime.InteropServices;

namespace Titan.Anpr.NativeInterop;

/// <summary>
/// P/Invoke bindings for the unified Titan-ANPR detection API (<c>titan_anpr.h</c>).
/// </summary>
public static class TitanAnprNative
{
    /// <summary>Corner point in pixel space (native <c>PlatePoint</c>).</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PlatePoint
    {
        public float x;
        public float y;
    }

    /// <summary>Result of <see cref="TitanANPR_Detect"/> (native <c>TitanAnprResult</c>).</summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TitanAnprResult
    {
        public int found;
        public float total_confidence;
        public float plate_confidence;
        public float ocr_confidence;
        public PlatePoint point0;
        public PlatePoint point1;
        public PlatePoint point2;
        public PlatePoint point3;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string plate_text;
    }

    [DllImport(NativeDll.DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "TitanANPR_Init")]
    public static extern int TitanANPR_Init(out IntPtr out_handle);

    [DllImport(NativeDll.DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "TitanANPR_Detect")]
    public static extern int TitanANPR_Detect(
        IntPtr handle,
        IntPtr img_data,
        int width,
        int height,
        int stride,
        ref TitanAnprResult out_result);

    [DllImport(NativeDll.DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "TitanANPR_Dispose")]
    public static extern void TitanANPR_Dispose(IntPtr handle);
}
