namespace TuckBar;

internal sealed class TuckBarApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly MessageWindow _messageWindow;
    private readonly ToolStripMenuItem _statusItem;

    public TuckBarApplicationContext()
    {
        _statusItem = new ToolStripMenuItem { Enabled = false };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_statusItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Toggle Auto-hide", null, OnToggleAutoHide);
        contextMenu.Items.Add("Exit", null, OnExit);

        _notifyIcon = new NotifyIcon
        {
            Icon = CreateIcon(),
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
    }

    private static Icon CreateIcon()
    {
        var bitmap = new Bitmap(16, 16);
        using Graphics g = Graphics.FromImage(bitmap);
        g.Clear(Color.FromArgb(64, 64, 64));

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
