using System;
using MayaRemote.Core.Interfaces;

namespace MayaRemote.WindowsServices.Audio;

public sealed class AudioService : IAudioService
{
    private const int APPCOMMAND_VOLUME_UP = 0x0A0000;
    private const int APPCOMMAND_VOLUME_DOWN = 0x090000;
    private const int APPCOMMAND_VOLUME_MUTE = 0x080000;
    private const int WM_APPCOMMAND = 0x319;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public Task SetVolumeAsync(float volume)
    {
        volume = Math.Clamp(volume, 0f, 1f);
        var steps = (int)(volume * 100);
        var hWnd = FindWindow("Shell_TrayWnd", null);
        var lParam = APPCOMMAND_VOLUME_UP | steps;
        SendMessage(hWnd, WM_APPCOMMAND, IntPtr.Zero, (IntPtr)lParam);
        return Task.CompletedTask;
    }

    public Task<float> GetVolumeAsync() => Task.FromResult(0.5f);

    public Task SetMuteAsync(bool muted)
    {
        var hWnd = FindWindow("Shell_TrayWnd", null);
        SendMessage(hWnd, WM_APPCOMMAND, IntPtr.Zero, (IntPtr)(APPCOMMAND_VOLUME_MUTE | (muted ? 0x800000 : 0)));
        return Task.CompletedTask;
    }

    public Task<bool> GetMuteAsync() => Task.FromResult(false);
}
