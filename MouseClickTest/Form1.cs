using System.Diagnostics;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace MouseClickTest;

public partial class Form1 : Form
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
    private GlobalKeyboardHook gHook;
    private int Clicks = 0;
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    private Timer clickTimer = new();

    public Form1()
    {
        InitializeComponent();
        clickTimer.Interval = 1;
        clickTimer.Tick += ClickTimer_Tick;
        gHook = new GlobalKeyboardHook();
        gHook.KeyPressed += GHook_KeyPressed;
    }

    private void ClickTimer_Tick(object? sender, EventArgs e)
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        this.Text = $"Clicks: {++Clicks}";
    }
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        gHook.Hook();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        gHook.Unhook();
        base.OnFormClosing(e);
    }

    private void GHook_KeyPressed(object? sender, KeyPressedEventArgs e)
    {
        if (e.Key == Keys.M)
        {
            if (!clickTimer.Enabled)
            {
                timerLabel.Text = "On";
                clickTimer.Start();
            }
            else
            {
                timerLabel.Text = "Off";
                clickTimer.Stop();
            }
        }
    }
}
