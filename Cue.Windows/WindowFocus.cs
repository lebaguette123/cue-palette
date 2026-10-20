namespace Cue.Windows;

public static class WindowFocus
{
    public static bool BringToForeground(nint hwnd)
    {
        if (NativeMethods.GetForegroundWindow() == hwnd) return true;
        
        NativeMethods.SetForegroundWindow(hwnd);
        if (NativeMethods.GetForegroundWindow() == hwnd) return true;
        
        var foreground = NativeMethods.GetForegroundWindow();
        var foregroundThread = foreground == 0 ? 0 : NativeMethods.GetWindowThreadProcessId(foreground, 0);
        var ourThread = NativeMethods.GetCurrentThreadId();

        var attached = foregroundThread != 0 && foregroundThread != ourThread &&
                       NativeMethods.AttachThreadInput(ourThread, foregroundThread, true);
        try
        {
            NativeMethods.BringWindowToTop(hwnd);
            NativeMethods.SetForegroundWindow(hwnd);
        }
        finally
        {
            if (attached) NativeMethods.AttachThreadInput(ourThread, foregroundThread, false);
        }

        return NativeMethods.GetForegroundWindow() == hwnd;
    }
}