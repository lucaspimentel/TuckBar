namespace TuckBar;

internal sealed class Settings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "TuckBar", "settings.yml");

    public Dictionary<string, bool> Monitors { get; } = new(StringComparer.OrdinalIgnoreCase);
    public bool RemoteDesktop { get; set; }

    public bool GetOrAddMonitor(string name, bool defaultHide)
    {
        if (Monitors.TryGetValue(name, out bool existing))
        {
            return existing;
        }

        Monitors[name] = defaultHide;
        return defaultHide;
    }

    public void SetMonitorHide(string name, bool hide) =>
        Monitors[name] = hide;

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
        bool inMonitorsSection = false;

        foreach (string line in text.Split('\n'))
        {
            string trimmed = line.TrimEnd('\r');

            // Check if this is an indented line (monitor entry)
            if (inMonitorsSection && trimmed.Length > 0 && trimmed[0] is ' ' or '\t')
            {
                string entry = trimmed.Trim();
                int lastColon = entry.LastIndexOf(':');
                if (lastColon > 0)
                {
                    string name = entry[..lastColon].Trim();
                    string value = entry[(lastColon + 1)..].Trim();

                    if (name.Length > 0 && bool.TryParse(value, out bool parsed))
                    {
                        settings.Monitors[name] = parsed;
                    }
                }

                continue;
            }

            // Non-indented line exits the monitors section
            inMonitorsSection = false;

            int colonIndex = trimmed.IndexOf(':');
            if (colonIndex < 0)
            {
                continue;
            }

            string key = trimmed[..colonIndex].Trim();
            string val = trimmed[(colonIndex + 1)..].Trim();

            if (key == "monitors" && val.Length == 0)
            {
                inMonitorsSection = true;
                continue;
            }

            if (key == "hide-when-remote-desktop" && bool.TryParse(val, out bool rdp))
            {
                settings.RemoteDesktop = rdp;
            }
            // Silently ignore old keys (hide-when-internal-only, etc.)
        }

        return settings;
    }

    internal string Serialize()
    {
        var lines = new List<string>
        {
            $"hide-when-remote-desktop: {RemoteDesktop.ToString().ToLowerInvariant()}"
        };

        if (Monitors.Count > 0)
        {
            lines.Add("monitors:");
            foreach ((string name, bool hide) in Monitors)
            {
                lines.Add($"  {name}: {hide.ToString().ToLowerInvariant()}");
            }
        }

        return string.Join('\n', lines) + '\n';
    }
}
