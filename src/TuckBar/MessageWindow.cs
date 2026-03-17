namespace TuckBar;

internal sealed class MessageWindow : Form
{
    private const int WM_DISPLAYCHANGE = 0x007E;
    private readonly System.Windows.Forms.Timer _debounceTimer;

    public event EventHandler? DisplayChanged;

    public MessageWindow()
    {
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        Size = Size.Empty;
        Opacity = 0;

        _debounceTimer = new System.Windows.Forms.Timer { Interval = 500 };
        _debounceTimer.Tick += OnDebounceTimerTick;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            const int WS_EX_TOOLWINDOW = 0x00000080;
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_DISPLAYCHANGE)
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        base.WndProc(ref m);
    }

    private void OnDebounceTimerTick(object? sender, EventArgs e)
    {
        _debounceTimer.Stop();
        DisplayChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _debounceTimer.Dispose();
        }

        base.Dispose(disposing);
    }
}
