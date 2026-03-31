using System.Drawing;
using System.Windows.Forms;

namespace Titan.Anpr.DetectionConsole;

/// <summary>
/// Single modal window for one overlay image. Closing it (any key or Alt+F4) must happen before the next image is shown.
/// </summary>
internal sealed class OverlayPreviewForm : Form
{
    private readonly PictureBox _pictureBox = new()
    {
        Dock = DockStyle.Fill,
        SizeMode = PictureBoxSizeMode.Zoom,
        BackColor = Color.Black
    };

    public OverlayPreviewForm(string imagePath)
    {
        Text = "Titan-ANPR overlay — press any key to continue (or close window)";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1100;
        Height = 800;
        KeyPreview = true;
        Controls.Add(_pictureBox);

        using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        _pictureBox.Image = Image.FromStream(stream);

        KeyDown += (_, _) => Close();
        Click += (_, _) => Close();
        _pictureBox.Click += (_, _) => Close();
        FormClosed += OnFormClosed;
    }

    private void OnFormClosed(object? sender, FormClosedEventArgs e)
    {
        FormClosed -= OnFormClosed;
        var img = _pictureBox.Image;
        _pictureBox.Image = null;
        img?.Dispose();
    }
}
