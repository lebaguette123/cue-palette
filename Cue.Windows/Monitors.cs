namespace Cue.Windows;

public static class Monitors
{
    private const uint MONITOR_DEFAULTTONEAREST = 2;
    private const int MDT_EFFECTIVE_DPI = 0;

    public static (int X, int Y) GetCursorPosition()
    {
        return NativeMethods.GetCursorPos(out var p) ? (p.X, p.Y) : (0, 0);
    }

    public static double GetScaleAt(int x, int y)
    {
        var monitor = NativeMethods.MonitorFromPoint(new NativeMethods.POINT { X = x, Y = y }, MONITOR_DEFAULTTONEAREST);
        if (monitor == 0) return 1.0;

        var hr = NativeMethods.GetDpiForMonitor(monitor, MDT_EFFECTIVE_DPI, out var dpiX, out _);
        return hr == 0 && dpiX > 0 ? dpiX / 96.0 : 1.0;
    }
}