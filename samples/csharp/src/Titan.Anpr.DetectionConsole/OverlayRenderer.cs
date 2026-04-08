using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Titan.Anpr.NativeInterop;

namespace Titan.Anpr.DetectionConsole;

/// <summary>
/// Loads images as 24 bpp BGR (GDI+ layout) for the native detector and draws plate overlays.
/// </summary>
internal static class OverlayRenderer
{
    private static readonly StringFormat CenterFormat = new()
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center
    };

    internal static Bitmap LoadAsRgb24(string path)
    {
        using var src = new Bitmap(path);
        var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb);
        using (var g = Graphics.FromImage(dst))
        {
            g.DrawImage(src, 0, 0, src.Width, src.Height);
        }

        return dst;
    }

    /// <summary>
    /// Draws all detected plates: oriented quadrilateral, axis-aligned bounding box and text label.
    /// </summary>
    internal static void DrawPlateOverlays(Graphics g, IReadOnlyList<TitanAnprNative.TitanAnprResult> results)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        for (var idx = 0; idx < results.Count; idx++)
        {
            var result = results[idx];
            var pts = new[]
            {
                new PointF(result.point0.x, result.point0.y),
                new PointF(result.point1.x, result.point1.y),
                new PointF(result.point2.x, result.point2.y),
                new PointF(result.point3.x, result.point3.y)
            };

            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var p in pts)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }

            using (var boxPen = new Pen(Color.FromArgb(220, 255, 140, 0), 2f) { DashStyle = DashStyle.Dash })
            {
                if (maxX > minX && maxY > minY)
                    g.DrawRectangle(boxPen, minX, minY, maxX - minX, maxY - minY);
            }

            using var quadPen = new Pen(Color.Lime, 3f);
            g.DrawPolygon(quadPen, pts);

            var text = string.IsNullOrWhiteSpace(result.plate_text) ? "(no text)" : result.plate_text.Trim('\0', ' ');
            var country = string.IsNullOrWhiteSpace(result.country_name)
                ? null
                : result.country_name.Trim('\0', ' ');
            var label = country is null ? $"#{idx + 1} {text}" : $"#{idx + 1} {text} [{country}]";
            using var font = new Font(FontFamily.GenericSansSerif, 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            using var fill = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
            using var outline = new Pen(Color.Lime, 2f);
            var textPos = new PointF((minX + maxX) / 2f, Math.Max(0f, minY - 8f));
            var size = g.MeasureString(label, font);
            var rect = new RectangleF(textPos.X - size.Width / 2f - 4f, textPos.Y - size.Height - 4f, size.Width + 8f, size.Height + 8f);
            g.FillRectangle(fill, rect);
            g.DrawRectangle(outline, rect.X, rect.Y, rect.Width, rect.Height);
            g.DrawString(label, font, Brushes.White, rect, CenterFormat);
        }
    }

    internal static void DrawNoPlateBanner(Graphics g, int width, int height)
    {
        using var font = new Font(FontFamily.GenericSansSerif, 16f, FontStyle.Bold, GraphicsUnit.Pixel);
        using var fill = new SolidBrush(Color.FromArgb(180, 40, 40, 40));
        var text = "No plate detected";
        var size = g.MeasureString(text, font);
        var rect = new RectangleF(8f, 8f, size.Width + 16f, size.Height + 12f);
        g.FillRectangle(fill, rect);
        g.DrawString(text, font, Brushes.White, rect, CenterFormat);
    }

    internal static void SavePng(Bitmap bitmap, string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        bitmap.Save(path, ImageFormat.Png);
    }
}
