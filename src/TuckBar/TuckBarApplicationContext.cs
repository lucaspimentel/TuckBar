namespace TuckBar;

internal sealed class TuckBarApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly MessageWindow _messageWindow;
    private readonly Settings _settings;
    private readonly ToolStripMenuItem _autoHideItem;
    private readonly ToolStripMenuItem _internalOnlyItem;
    private readonly ToolStripMenuItem _externalOnlyItem;
    private readonly ToolStripMenuItem _bothItem;
    private readonly ToolStripMenuItem _startupItem;

    public TuckBarApplicationContext()
    {
        _settings = Settings.Load();

        _autoHideItem = new ToolStripMenuItem("Auto-hide (temporary)");
        _autoHideItem.Click += OnToggleAutoHide;

        _internalOnlyItem = new ToolStripMenuItem("Internal monitor only") { CheckOnClick = true, Checked = _settings.InternalOnly };
        _externalOnlyItem = new ToolStripMenuItem("External monitor only") { CheckOnClick = true, Checked = _settings.ExternalOnly };
        _bothItem = new ToolStripMenuItem("Both monitors") { CheckOnClick = true, Checked = _settings.Both };

        _internalOnlyItem.CheckedChanged += OnSettingChanged;
        _externalOnlyItem.CheckedChanged += OnSettingChanged;
        _bothItem.CheckedChanged += OnSettingChanged;

        _startupItem = new ToolStripMenuItem("Start with Windows")
        {
            Checked = StartupHelper.IsEnabled()
        };
        _startupItem.Click += OnToggleStartup;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_autoHideItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_internalOnlyItem);
        contextMenu.Items.Add(_externalOnlyItem);
        contextMenu.Items.Add(_bothItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_startupItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, OnExit);

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = contextMenu,
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
        (bool hasInternal, bool hasExternal) = DisplayMonitor.GetDisplayState();

        bool shouldAutoHide = (hasInternal, hasExternal) switch
        {
            (true, false) => _settings.InternalOnly,
            (false, true) => _settings.ExternalOnly,
            (true, true) => _settings.Both,
            _ => _settings.InternalOnly // no displays detected, treat as internal
        };

        TaskbarHelper.SetAutoHide(shouldAutoHide);
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        bool autoHide = TaskbarHelper.GetAutoHide();
        _autoHideItem.Checked = autoHide;
        _notifyIcon.Text = $"TuckBar - Auto-hide: {(autoHide ? "ON" : "OFF")}";

        Icon? oldIcon = _notifyIcon.Icon;
        _notifyIcon.Icon = CreateIcon(autoHide);

        if (oldIcon is not null)
        {
            DestroyIcon(oldIcon);
        }
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
