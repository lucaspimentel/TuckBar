using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Devices.Display;
using Windows.Win32.Foundation;

namespace TuckBar;

internal record DisplayInfo(string Name, bool IsInternal);

internal static class DisplayMonitor
{
    public static List<DisplayInfo> GetDisplays()
    {
        var displays = new List<DisplayInfo>();

        WIN32_ERROR result = PInvoke.GetDisplayConfigBufferSizes(
            QUERY_DISPLAY_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
            out uint pathCount,
            out uint modeCount);

        if (result != WIN32_ERROR.ERROR_SUCCESS)
        {
            return displays;
        }

        var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

        result = PInvoke.QueryDisplayConfig(
            QUERY_DISPLAY_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
            ref pathCount,
            paths,
            ref modeCount,
            modes);

        if (result != WIN32_ERROR.ERROR_SUCCESS)
        {
            return displays;
        }

        for (int i = 0; i < pathCount; i++)
        {
            DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY tech = paths[i].targetInfo.outputTechnology;
            bool isInternal = IsInternalOutput(tech);
            string name = GetMonitorName(paths[i].targetInfo.adapterId, paths[i].targetInfo.id);

            displays.Add(new DisplayInfo(name, isInternal));
        }

        return displays;
    }

    private static string GetMonitorName(LUID adapterId, uint targetId)
    {
        var deviceName = new DISPLAYCONFIG_TARGET_DEVICE_NAME
        {
            header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
            {
                type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME,
                size = (uint)Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>(),
                adapterId = adapterId,
                id = targetId
            }
        };

        int result = PInvoke.DisplayConfigGetDeviceInfo(ref deviceName.header);

        if (result != (int)WIN32_ERROR.ERROR_SUCCESS)
        {
            return "Unknown";
        }

        string name = deviceName.monitorFriendlyDeviceName.ToString();

        if (string.IsNullOrWhiteSpace(name))
        {
            name = SystemInformation.TerminalServerSession ? "Remote Desktop" : "Unknown";
        }

        return name;
    }

    internal static bool IsInternalOutput(DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY tech) =>
        tech is DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL
            or DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED
            or DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED;
}
