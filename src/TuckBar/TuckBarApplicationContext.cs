namespace TuckBar;

internal sealed class TuckBarApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly MessageWindow _messageWindow;
    private readonly Settings _settings;
    private readonly ToolStripMenuItem _autoHideItem;
    private readonly ToolStripMenuItem _rdpItem;
    private readonly ToolStripMenuItem _startupItem;
    private readonly int _monitorItemsIndex;

    public TuckBarApplicationContext()
    {
        _settings = Settings.Load();

        _autoHideItem = new ToolStripMenuItem("Auto-hide (temporary)");
        _autoHideItem.Click += OnToggleAutoHide;

        _rdpItem = new ToolStripMenuItem("Hide when: Remote Desktop") { CheckOnClick = true, Checked = _settings.RemoteDesktop };
        _rdpItem.CheckedChanged += OnRdpSettingChanged;

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

    private void OnRdpSettingChanged(object? sender, EventArgs e)
    {
        _settings.RemoteDesktop = _rdpItem.Checked;
        _settings.Save();
        EvaluateAndApply();
    }

    private void OnMonitorSettingChanged(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem { Tag: string name } item)
        {
            _settings.SetMonitorHide(name, item.Checked);
            _settings.Save();
            EvaluateAndApply();
        }
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
        else if (displays.Count == 0)
        {
            shouldAutoHide = true;
        }
        else
        {
            bool currentAutoHide = TaskbarHelper.GetAutoHide();

            // Ensure all connected monitors are in settings (adds new ones with current state as default)
            foreach (DisplayInfo display in displays)
            {
                _settings.GetOrAddMonitor(display.Name, currentAutoHide);
            }

            // Hide if ANY connected monitor has hide=true
            shouldAutoHide = displays.Any(d => _settings.Monitors[d.Name]);
            _settings.Save();
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

        var connectedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int insertIndex = _monitorItemsIndex;

        // Add connected monitors as checkboxes
        foreach (DisplayInfo display in displays)
        {
            connectedNames.Add(display.Name);
            string type = display.IsInternal ? "Internal" : "External";
            bool hide = _settings.Monitors.GetValueOrDefault(display.Name);

            var item = new ToolStripMenuItem($"{display.Name} ({type})")
            {
                CheckOnClick = true,
                Checked = hide,
                Tag = display.Name
            };
            item.CheckedChanged += OnMonitorSettingChanged;
            _contextMenu.Items.Insert(insertIndex++, item);
        }

        // Add disconnected monitors (in settings but not currently connected)
        foreach ((string name, bool hide) in _settings.Monitors)
        {
            if (connectedNames.Contains(name))
            {
                continue;
            }

            var item = new ToolStripMenuItem($"{name} (disconnected)")
            {
                CheckOnClick = true,
                Checked = hide,
                Tag = name
            };
            item.CheckedChanged += OnMonitorSettingChanged;
            _contextMenu.Items.Insert(insertIndex++, item);
        }

        if (insertIndex == _monitorItemsIndex)
        {
            var item = new ToolStripMenuItem("No monitors detected") { Enabled = false };
            _contextMenu.Items.Insert(_monitorItemsIndex, item);
        }
    }

    private void UpdateTooltip(bool autoHide, List<DisplayInfo> displays)
    {
        string text = $"TuckBar - Auto-hide: {(autoHide ? "ON" : "OFF")} ({displays.Count} monitor{(displays.Count == 1 ? "" : "s")} connected)";

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
