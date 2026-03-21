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
            internal-only: false
            external-only: true
            both: false
            remote-desktop: true
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
            internal-only: false
            unknown-key: true
            both: false
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
            internal-only: maybe
            external-only: 1
            both: false
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
            internal-only: true
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
            internal-only: true
            external-only: false
            both: true
            remote-desktop: false
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
