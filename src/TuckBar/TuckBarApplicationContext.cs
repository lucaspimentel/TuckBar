namespace TuckBar;

internal sealed class TuckBarApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly MessageWindow _messageWindow;
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _startupItem;

    public TuckBarApplicationContext()
    {
        _statusItem = new ToolStripMenuItem { Enabled = false };
        _startupItem = new ToolStripMenuItem("Start with Windows")
        {
            Checked = StartupHelper.IsEnabled()
        };
        _startupItem.Click += OnToggleStartup;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_statusItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Toggle Auto-hide", null, OnToggleAutoHide);
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

        // Internal only or both: auto-hide ON
        // External only: auto-hide OFF
        bool shouldAutoHide = hasInternal || !hasExternal;

        TaskbarHelper.SetAutoHide(shouldAutoHide);
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        bool autoHide = TaskbarHelper.GetAutoHide();
        string state = autoHide ? "ON" : "OFF";
        _statusItem.Text = $"Auto-hide: {state}";
        _notifyIcon.Text = $"TuckBar - Auto-hide: {state}";

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
