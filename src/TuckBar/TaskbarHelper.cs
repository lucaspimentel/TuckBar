using System.Runtime.InteropServices;
using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace TuckBar;

internal static class TaskbarHelper
{
    private const string StuckRects3Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3";
    private const string SettingsValueName = "Settings";
    private const int AutoHideByteIndex = 8;
    private const byte AutoHideBitMask = 0x01;

    private const uint ABM_GETSTATE = 0x00000004;
    private const uint ABM_SETSTATE = 0x0000000A;
    private const int ABS_AUTOHIDE = 0x0000001;
    private const int ABS_ALWAYSONTOP = 0x0000002;

    internal static bool IsAutoHideEnabled(byte[] settingsBlob)
    {
        if (settingsBlob.Length <= AutoHideByteIndex)
        {
            return false;
        }

        return (settingsBlob[AutoHideByteIndex] & AutoHideBitMask) != 0;
    }

    internal static byte[] SetAutoHideBit(byte[] settingsBlob, bool autoHide)
    {
        var result = (byte[])settingsBlob.Clone();

        if (result.Length <= AutoHideByteIndex)
        {
            return result;
        }

        if (autoHide)
        {
            result[AutoHideByteIndex] |= AutoHideBitMask;
        }
        else
        {
            result[AutoHideByteIndex] &= unchecked((byte)~AutoHideBitMask);
        }

        return result;
    }

    public static bool GetAutoHide()
    {
        var abd = new APPBARDATA { cbSize = (uint)Marshal.SizeOf<APPBARDATA>() };
        nuint state = PInvoke.SHAppBarMessage(ABM_GETSTATE, ref abd);
        return ((int)state & ABS_AUTOHIDE) != 0;
    }

    public static void SetAutoHide(bool enable)
    {
        bool currentState = GetAutoHide();
        if (currentState == enable)
        {
            return;
        }

        // Update registry so the setting persists across Explorer restarts
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StuckRects3Path, writable: true);
        if (key?.GetValue(SettingsValueName) is byte[] blob)
        {
            byte[] updated = SetAutoHideBit(blob, enable);
            key.SetValue(SettingsValueName, updated, RegistryValueKind.Binary);
        }

        // Apply immediately via shell API
        var abd = new APPBARDATA
        {
            cbSize = (uint)Marshal.SizeOf<APPBARDATA>(),
            hWnd = PInvoke.FindWindow("Shell_TrayWnd", null),
            lParam = enable ? ABS_AUTOHIDE : ABS_ALWAYSONTOP
        };

        PInvoke.SHAppBarMessage(ABM_SETSTATE, ref abd);
    }
}
