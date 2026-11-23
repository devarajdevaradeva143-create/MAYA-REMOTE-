using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MayaRemote.Core.Interfaces;
using MayaRemote.Core.Models;

namespace MayaRemote.WindowsServices.Input;

public sealed class InputSimulator : IInputService, IDisposable
{
    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    [DllImport("user32.dll")]
    private static extern short MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUT
    {
        [FieldOffset(0)] public uint type;
        [FieldOffset(4)] public MOUSEINPUT mi;
        [FieldOffset(4)] public KEYBDINPUT ki;
        [FieldOffset(4)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

    public Task SendKeyAsync(ushort virtualKey, bool keyDown)
    {
        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            ki = new KEYBDINPUT { wVk = virtualKey, dwFlags = keyDown ? (uint)0 : KEYEVENTF_KEYUP }
        };
        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        return Task.CompletedTask;
    }

    public Task SendMouseAsync(int x, int y, uint flags)
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,
            mi = new MOUSEINPUT { dx = x, dy = y, dwFlags = flags }
        };
        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        return Task.CompletedTask;
    }

    public void Dispose() { }
}
