namespace TuckBar;

internal sealed class TuckBarApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly MessageWindow _messageWindow;
    private readonly Settings _settings;
    private readonly ToolStripMenuItem _autoHideItem;
    private readonly ToolStripMenuItem _internalOnlyItem;
    private readonly ToolStripMenuItem _externalOnlyItem;
    private readonly ToolStripMenuItem _bothItem;
    private readonly ToolStripMenuItem _rdpItem;
    private readonly ToolStripMenuItem _startupItem;
    private readonly int _monitorItemsIndex;

    public TuckBarApplicationContext()
    {
        _settings = Settings.Load();

        _autoHideItem = new ToolStripMenuItem("Auto-hide (temporary)");
        _autoHideItem.Click += OnToggleAutoHide;

        _internalOnlyItem = new ToolStripMenuItem("Hide when: internal monitor only") { CheckOnClick = true, Checked = _settings.InternalOnly };
        _externalOnlyItem = new ToolStripMenuItem("Hide when: external monitor only") { CheckOnClick = true, Checked = _settings.ExternalOnly };
        _bothItem = new ToolStripMenuItem("Hide when: both monitors") { CheckOnClick = true, Checked = _settings.Both };
        _rdpItem = new ToolStripMenuItem("Hide when: Remote Desktop") { CheckOnClick = true, Checked = _settings.RemoteDesktop };

        _internalOnlyItem.CheckedChanged += OnSettingChanged;
        _externalOnlyItem.CheckedChanged += OnSettingChanged;
        _bothItem.CheckedChanged += OnSettingChanged;
        _rdpItem.CheckedChanged += OnSettingChanged;

        _startupItem = new ToolStripMenuItem("Start with Windows")
        {
            Checked = StartupHelper.IsEnabled()
        };
        _startupItem.Click += OnToggleStartup;

        _contextMenu = new ContextMenuStrip();
        _monitorItemsIndex = _contextMenu.Items.Count; // monitor items inserted here
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_autoHideItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_internalOnlyItem);
        _contextMenu.Items.Add(_externalOnlyItem);
        _contextMenu.Items.Add(_bothItem);
        _contextMenu.Items.Add(_rdpItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_startupItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add("Exit", null, OnExit);

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = _contextMenu,
            Visible = true
        };

        _messageWindow = new MessageWindow();
        _messageWindow.DisplayChanged += OnDisplayChanged;
        _messageWindow.Show();
        _messageWindow.Hide();

        EvaluateAndApply();
    }

    private void OnDisplayChanged(object? sender, EventArgs e) =>
        EvaluateAndApply();

    private void OnToggleStartup(object? sender, EventArgs e)
    {
        bool enable = !StartupHelper.IsEnabled();
        StartupHelper.SetEnabled(enable);
        _startupItem.Checked = enable;
    }

    private void OnSettingChanged(object? sender, EventArgs e)
    {
        _settings.InternalOnly = _internalOnlyItem.Checked;
        _settings.ExternalOnly = _externalOnlyItem.Checked;
        _settings.Both = _bothItem.Checked;
        _settings.RemoteDesktop = _rdpItem.Checked;
        _settings.Save();
        EvaluateAndApply();
    }

    private void OnToggleAutoHide(object? sender, EventArgs e)
    {
        bool current = TaskbarHelper.GetAutoHide();
        TaskbarHelper.SetAutoHide(!current);
        UpdateStatus();
    }

    private void OnExit(object? sender, EventArgs e)
    {
        _notifyIcon.Visible = false;
        Application.Exit();
    }

    private void EvaluateAndApply()
    {
        List<DisplayInfo> displays = DisplayMonitor.GetDisplays();

        bool shouldAutoHide;

        if (SystemInformation.TerminalServerSession)
        {
            shouldAutoHide = _settings.RemoteDesktop;
        }
        else
        {
            bool hasInternal = displays.Any(d => d.IsInternal);
            bool hasExternal = displays.Any(d => !d.IsInternal);

            shouldAutoHide = (hasInternal, hasExternal) switch
            {
                (true, false) => _settings.InternalOnly,
                (false, true) => _settings.ExternalOnly,
                (true, true) => _settings.Both,
                _ => _settings.InternalOnly // no displays detected, treat as internal
            };
        }

        TaskbarHelper.SetAutoHide(shouldAutoHide);
        UpdateStatus(displays);
    }

    private void UpdateStatus(List<DisplayInfo>? displays = null)
    {
        bool autoHide = TaskbarHelper.GetAutoHide();
        _autoHideItem.Checked = autoHide;

        displays ??= DisplayMonitor.GetDisplays();
        UpdateMonitorMenuItems(displays);
        UpdateTooltip(autoHide, displays);

        Icon? oldIcon = _notifyIcon.Icon;
        _notifyIcon.Icon = CreateIcon(autoHide);

        if (oldIcon is not null)
        {
            DestroyIcon(oldIcon);
        }
    }

    private void UpdateMonitorMenuItems(List<DisplayInfo> displays)
    {
        // Remove existing monitor items (everything before the first separator)
        while (_monitorItemsIndex < _contextMenu.Items.Count &&
               _contextMenu.Items[_monitorItemsIndex] is not ToolStripSeparator)
        {
            _contextMenu.Items.RemoveAt(_monitorItemsIndex);
        }

        // Insert new monitor items
        for (int i = 0; i < displays.Count; i++)
        {
            DisplayInfo display = displays[i];
            string type = display.IsInternal ? "Internal" : "External";
            var item = new ToolStripMenuItem($"{display.Name} ({type})") { Enabled = false };
            _contextMenu.Items.Insert(_monitorItemsIndex + i, item);
        }

        if (displays.Count == 0)
        {
            var item = new ToolStripMenuItem("No monitors detected") { Enabled = false };
            _contextMenu.Items.Insert(_monitorItemsIndex, item);
        }
    }

    private void UpdateTooltip(bool autoHide, List<DisplayInfo> displays)
    {
        int internalCount = displays.Count(d => d.IsInternal);
        int externalCount = displays.Count(d => !d.IsInternal);

        string monitorSummary = (internalCount, externalCount) switch
        {
            (> 0, > 0) => $"internal + {externalCount} external",
            (> 0, 0) => "internal only",
            (0, > 0) => $"{externalCount} external",
            _ => "no monitors"
        };

        string text = $"TuckBar - Auto-hide: {(autoHide ? "ON" : "OFF")} ({monitorSummary})";

        // NotifyIcon.Text is limited to 127 characters
        if (text.Length > 127)
        {
            text = text[..127];
        }

        _notifyIcon.Text = text;
    }

    private static Icon CreateIcon(bool autoHide)
    {
        Color bg = autoHide ? Color.FromArgb(0, 122, 204) : Color.FromArgb(64, 64, 64);

        var bitmap = new Bitmap(16, 16);
        using Graphics g = Graphics.FromImage(bitmap);
        g.Clear(bg);

        using var font = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(Color.White);
        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        g.DrawString("T", font, brush, new RectangleF(0, 0, 16, 16), format);

        return Icon.FromHandle(bitmap.GetHicon());
    }

    private static void DestroyIcon(Icon icon) =>
        icon.Dispose();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notifyIcon.Dispose();
            _messageWindow.Dispose();
        }

        base.Dispose(disposing);
    }
}
