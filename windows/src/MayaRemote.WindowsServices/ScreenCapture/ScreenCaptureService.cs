using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using MayaRemote.Core.Interfaces;

namespace MayaRemote.WindowsServices.ScreenCapture;

public sealed class ScreenCaptureService : IScreenCaptureService
{
    public Task<byte[]> CaptureScreenAsync(CancellationToken cancellationToken = default)
    {
        var bounds = GetVirtualScreenBounds();
        return Task.FromResult(Capture(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom));
    }

    public Task<byte[]> CaptureRegionAsync(int left, int top, int right, int bottom, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Capture(left, top, right, bottom));
    }

    private static byte[] Capture(int left, int top, int right, int bottom)
    {
        var width = right - left;
        var height = bottom - top;
        if (width <= 0 || height <= 0) return Array.Empty<byte>();

        using var bitmap = new Bitmap(width, height);
        using var g = Graphics.FromImage(bitmap);
        g.CopyFromScreen(left, top, 0, 0, new Size(width, height));
        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    private static Rectangle GetVirtualScreenBounds()
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;
        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            var b = screen.Bounds;
            minX = Math.Min(minX, b.Left);
            minY = Math.Min(minY, b.Top);
            maxX = Math.Max(maxX, b.Right);
            maxY = Math.Max(maxY, b.Bottom);
        }
        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }
}
