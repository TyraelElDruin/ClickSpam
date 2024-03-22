using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MouseClickTest;
public class GlobalKeyboardHook : IDisposable
{
    public event EventHandler<KeyPressedEventArgs> KeyPressed = null!;
    private static IntPtr hookId = IntPtr.Zero;
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYUP = 0x0101;
    private readonly LowLevelKeyboardProc callback;
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    public GlobalKeyboardHook()
    {
        callback = HookCallback;
        hookId = SetHook(callback);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule!)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            KeyPressed?.Invoke(this, new KeyPressedEventArgs((Keys)vkCode));
            // Stop the key from being passed on to the rest of the system
            return (IntPtr)1;
        }
        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }
    public void Hook()
    {
        hookId = SetHook(callback);
    }
    public void Unhook()
    {
        UnhookWindowsHookEx(hookId);
    }
    public void Dispose()
    {
        Unhook();
    }
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}

public class KeyPressedEventArgs : EventArgs
{
    public Keys Key { get; private set; }

    public KeyPressedEventArgs(Keys key)
    {
        Key = key;
    }
}
