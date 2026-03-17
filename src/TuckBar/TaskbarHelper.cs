using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TuckBar;

internal static class TaskbarHelper
{
    private const string StuckRects3Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3";
    private const string SettingsValueName = "Settings";
    private const int AutoHideByteIndex = 8;
    private const byte AutoHideBitMask = 0x01;

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
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StuckRects3Path);
        if (key?.GetValue(SettingsValueName) is not byte[] blob)
        {
            return false;
        }

        return IsAutoHideEnabled(blob);
    }

    public static void SetAutoHide(bool enable)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StuckRects3Path, writable: true);
        if (key?.GetValue(SettingsValueName) is not byte[] blob)
        {
            return;
        }

        bool currentState = IsAutoHideEnabled(blob);
        if (currentState == enable)
        {
            return;
        }

        byte[] updated = SetAutoHideBit(blob, enable);
        key.SetValue(SettingsValueName, updated, RegistryValueKind.Binary);

        BroadcastSettingChange();
    }

    private static void BroadcastSettingChange()
    {
        HWND taskbar = PInvoke.FindWindow("Shell_TrayWnd", null);
        if (taskbar != HWND.Null)
        {
            PInvoke.SendMessageTimeout(
                taskbar,
                0x001A, // WM_SETTINGCHANGE
                0,
                default,
                SEND_MESSAGE_TIMEOUT_FLAGS.SMTO_ABORTIFHUNG,
                2000,
                out _);
        }
    }
}
