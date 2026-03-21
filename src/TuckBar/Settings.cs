namespace TuckBar;

internal sealed class Settings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "TuckBar", "settings.yml");

    public bool InternalOnly { get; set; } = true;
    public bool ExternalOnly { get; set; }
    public bool Both { get; set; } = true;
    public bool RemoteDesktop { get; set; }

    public static Settings Load()
    {
        if (!File.Exists(FilePath))
        {
            return new Settings();
        }

        return Parse(File.ReadAllText(FilePath));
    }

    public void Save()
    {
        string? directory = Path.GetDirectoryName(FilePath);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(FilePath, Serialize());
    }

    internal static Settings Parse(string text)
    {
        var settings = new Settings();

        foreach (string line in text.Split('\n'))
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex < 0)
            {
                continue;
            }

            string key = line[..colonIndex].Trim();
            string value = line[(colonIndex + 1)..].Trim();

            if (!bool.TryParse(value, out bool parsed))
            {
                continue;
            }

            switch (key)
            {
                case "internal-only":
                    settings.InternalOnly = parsed;
                    break;
                case "external-only":
                    settings.ExternalOnly = parsed;
                    break;
                case "both":
                    settings.Both = parsed;
                    break;
                case "remote-desktop":
                    settings.RemoteDesktop = parsed;
                    break;
            }
        }

        return settings;
    }

    internal string Serialize() =>
        $"""
         internal-only: {InternalOnly.ToString().ToLowerInvariant()}
         external-only: {ExternalOnly.ToString().ToLowerInvariant()}
         both: {Both.ToString().ToLowerInvariant()}
         remote-desktop: {RemoteDesktop.ToString().ToLowerInvariant()}
         """;
}
