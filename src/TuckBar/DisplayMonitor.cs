using Windows.Win32;
using Windows.Win32.Devices.Display;
using Windows.Win32.Foundation;

namespace TuckBar;

internal static class DisplayMonitor
{
    public static (bool HasInternal, bool HasExternal) GetDisplayState()
    {
        bool hasInternal = false;
        bool hasExternal = false;

        WIN32_ERROR result = PInvoke.GetDisplayConfigBufferSizes(
            QUERY_DISPLAY_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
            out uint pathCount,
            out uint modeCount);

        if (result != WIN32_ERROR.ERROR_SUCCESS)
        {
            return (false, false);
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
            return (false, false);
        }

        for (int i = 0; i < pathCount; i++)
        {
            DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY tech = paths[i].targetInfo.outputTechnology;

            if (IsInternalOutput(tech))
            {
                hasInternal = true;
            }
            else
            {
                hasExternal = true;
            }
        }

        return (hasInternal, hasExternal);
    }

    internal static bool IsInternalOutput(DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY tech) =>
        tech is DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL
            or DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED
            or DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED;
}
