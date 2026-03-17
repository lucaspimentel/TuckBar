using Windows.Win32.Devices.Display;

namespace TuckBar.Tests;

public class DisplayMonitorTests
{
    [Theory]
    [InlineData(-2147483648, true)]  // INTERNAL
    [InlineData(11, true)]           // DISPLAYPORT_EMBEDDED
    [InlineData(13, true)]           // UDI_EMBEDDED
    [InlineData(5, false)]           // HDMI
    [InlineData(10, false)]          // DISPLAYPORT_EXTERNAL
    [InlineData(4, false)]           // DVI
    [InlineData(0, false)]           // HD15
    [InlineData(-1, false)]          // OTHER
    [InlineData(6, false)]           // LVDS
    public void IsInternalOutput_ClassifiesCorrectly(int techValue, bool expectedInternal) =>
        Assert.Equal(
            expectedInternal,
            DisplayMonitor.IsInternalOutput((DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY)techValue));
}
