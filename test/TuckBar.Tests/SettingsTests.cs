namespace TuckBar.Tests;

public class SettingsTests
{
    [Fact]
    public void Parse_DefaultValues_WhenEmpty()
    {
        Settings settings = Settings.Parse("");

        Assert.True(settings.InternalOnly);
        Assert.False(settings.ExternalOnly);
        Assert.True(settings.Both);
        Assert.False(settings.RemoteDesktop);
    }

    [Fact]
    public void Parse_ReadsAllValues()
    {
        const string yaml = """
            hide-when-internal-only: false
            hide-when-external-only: true
            hide-when-both: false
            hide-when-remote-desktop: true
            """;

        Settings settings = Settings.Parse(yaml);

        Assert.False(settings.InternalOnly);
        Assert.True(settings.ExternalOnly);
        Assert.False(settings.Both);
        Assert.True(settings.RemoteDesktop);
    }

    [Fact]
    public void Parse_IgnoresUnknownKeys()
    {
        const string yaml = """
            hide-when-internal-only: false
            unknown-key: true
            hide-when-both: false
            """;

        Settings settings = Settings.Parse(yaml);

        Assert.False(settings.InternalOnly);
        Assert.False(settings.ExternalOnly);
        Assert.False(settings.Both);
    }

    [Fact]
    public void Parse_IgnoresInvalidValues()
    {
        const string yaml = """
            hide-when-internal-only: maybe
            hide-when-external-only: 1
            hide-when-both: false
            """;

        Settings settings = Settings.Parse(yaml);

        Assert.True(settings.InternalOnly);   // default
        Assert.False(settings.ExternalOnly);  // default
        Assert.False(settings.Both);
    }

    [Fact]
    public void Parse_IgnoresMalformedLines()
    {
        const string yaml = """
            no-colon-here
            hide-when-internal-only: true
            """;

        Settings settings = Settings.Parse(yaml);

        Assert.True(settings.InternalOnly);
    }

    [Fact]
    public void Serialize_ProducesExpectedYaml()
    {
        var settings = new Settings
        {
            InternalOnly = true,
            ExternalOnly = false,
            Both = true,
            RemoteDesktop = false
        };

        string result = settings.Serialize();

        Assert.Equal(
            """
            hide-when-internal-only: true
            hide-when-external-only: false
            hide-when-both: true
            hide-when-remote-desktop: false
            """,
            result);
    }

    [Fact]
    public void Serialize_ThenParse_RoundTrips()
    {
        var original = new Settings
        {
            InternalOnly = false,
            ExternalOnly = true,
            Both = false,
            RemoteDesktop = true
        };

        Settings roundTripped = Settings.Parse(original.Serialize());

        Assert.Equal(original.InternalOnly, roundTripped.InternalOnly);
        Assert.Equal(original.ExternalOnly, roundTripped.ExternalOnly);
        Assert.Equal(original.Both, roundTripped.Both);
        Assert.Equal(original.RemoteDesktop, roundTripped.RemoteDesktop);
    }
}
