namespace TuckBar.Tests;

public class SettingsTests
{
    [Fact]
    public void Parse_DefaultValues_WhenEmpty()
    {
        Settings settings = Settings.Parse("");

        Assert.Empty(settings.Monitors);
        Assert.False(settings.RemoteDesktop);
    }

    [Fact]
    public void Parse_ReadsMonitors()
    {
        const string yaml = """
            hide-when-remote-desktop: true
            monitors:
              Built-in Display: true
              DELL U2722D: false
              LG 27UK850: true
            """;

        Settings settings = Settings.Parse(yaml);

        Assert.True(settings.RemoteDesktop);
        Assert.Equal(3, settings.Monitors.Count);
        Assert.True(settings.Monitors["Built-in Display"]);
        Assert.False(settings.Monitors["DELL U2722D"]);
        Assert.True(settings.Monitors["LG 27UK850"]);
    }

    [Theory]
    [InlineData("hide-when-internal-only: true")]
    [InlineData("hide-when-external-only: true")]
    [InlineData("hide-when-both: true")]
    public void Parse_IgnoresOldFormatKeys(string oldKey)
    {
        Settings settings = Settings.Parse(oldKey);

        Assert.Empty(settings.Monitors);
        Assert.False(settings.RemoteDesktop);
    }

    [Fact]
    public void Parse_MonitorNameWithColon()
    {
        const string yaml = """
            monitors:
              Samsung: C27F390: true
            """;

        Settings settings = Settings.Parse(yaml);

        Assert.Single(settings.Monitors);
        Assert.True(settings.Monitors["Samsung: C27F390"]);
    }

    [Fact]
    public void Parse_IgnoresMalformedLines()
    {
        const string yaml = """
            no-colon-here
            hide-when-remote-desktop: true
            """;

        Settings settings = Settings.Parse(yaml);

        Assert.True(settings.RemoteDesktop);
    }

    [Fact]
    public void Parse_IgnoresInvalidBoolValues()
    {
        const string yaml = """
            hide-when-remote-desktop: maybe
            monitors:
              Monitor1: yes
              Monitor2: true
            """;

        Settings settings = Settings.Parse(yaml);

        Assert.False(settings.RemoteDesktop); // default
        Assert.Single(settings.Monitors);     // Monitor1 skipped
        Assert.True(settings.Monitors["Monitor2"]);
    }

    [Fact]
    public void Serialize_ProducesExpectedYaml()
    {
        var settings = new Settings { RemoteDesktop = false };
        settings.Monitors["Built-in Display"] = true;
        settings.Monitors["DELL U2722D"] = false;

        string result = settings.Serialize();

        Assert.Equal(
            "hide-when-remote-desktop: false\n" +
            "monitors:\n" +
            "  Built-in Display: true\n" +
            "  DELL U2722D: false\n",
            result);
    }

    [Fact]
    public void Serialize_EmptyMonitors_OmitsSection()
    {
        var settings = new Settings { RemoteDesktop = true };

        string result = settings.Serialize();

        Assert.Equal("hide-when-remote-desktop: true\n", result);
        Assert.DoesNotContain("monitors:", result);
    }

    [Fact]
    public void Serialize_ThenParse_RoundTrips()
    {
        var original = new Settings { RemoteDesktop = true };
        original.Monitors["Built-in Display"] = true;
        original.Monitors["DELL U2722D"] = false;
        original.Monitors["LG 27UK850"] = true;

        Settings roundTripped = Settings.Parse(original.Serialize());

        Assert.Equal(original.RemoteDesktop, roundTripped.RemoteDesktop);
        Assert.Equal(original.Monitors.Count, roundTripped.Monitors.Count);
        foreach ((string name, bool hide) in original.Monitors)
        {
            Assert.Equal(hide, roundTripped.Monitors[name]);
        }
    }

    [Fact]
    public void GetOrAddMonitor_AddsNewMonitor()
    {
        var settings = new Settings();

        bool result = settings.GetOrAddMonitor("New Monitor", true);

        Assert.True(result);
        Assert.True(settings.Monitors["New Monitor"]);
    }

    [Fact]
    public void GetOrAddMonitor_ReturnsExistingValue()
    {
        var settings = new Settings();
        settings.Monitors["Existing"] = false;

        bool result = settings.GetOrAddMonitor("Existing", true);

        Assert.False(result);
        Assert.False(settings.Monitors["Existing"]);
    }

    [Fact]
    public void SetMonitorHide_UpdatesValue()
    {
        var settings = new Settings();
        settings.Monitors["Monitor"] = false;

        settings.SetMonitorHide("Monitor", true);

        Assert.True(settings.Monitors["Monitor"]);
    }
}
