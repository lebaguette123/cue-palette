namespace Cue.Windows;

public static class WindowChrome
{
    public static void UseSquareCorners(nint hwnd)
    {
        const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        var doNotRound = 1;
        NativeMethods.DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref doNotRound, sizeof(int));
    }

    public static void StripFrame(nint hwnd)
    {
        const int GWL_STYLE = -16, GWL_EXSTYLE = -20;
        const nint WS_CAPTION = 0x00C00000;
        const nint WS_THICKFRAME = 0x00040000;
        const nint WS_EX_DLGMODALFRAME = 0x00000001;
        const nint WS_EX_WINDOWEDGE = 0x00000100;
        const nint WS_EX_CLIENTEDGE = 0x00000200;
        const nint WS_EX_STATICEDGE = 0x00020000;
        const uint SWP_NOSIZE = 0x1, SWP_NOMOVE = 0x2, SWP_NOZORDER = 0x4,
            SWP_NOACTIVATE = 0x10, SWP_FRAMECHANGED = 0x20;

        var style = NativeMethods.GetWindowLongPtr(hwnd, GWL_STYLE);
        NativeMethods.SetWindowLongPtr(hwnd, GWL_STYLE, style & ~(WS_CAPTION | WS_THICKFRAME));
        
        var ex = NativeMethods.GetWindowLongPtr(hwnd, GWL_EXSTYLE);
        NativeMethods.SetWindowLongPtr(hwnd, GWL_EXSTYLE,
            ex & ~(WS_EX_DLGMODALFRAME | WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE));

        NativeMethods.SetWindowPos(hwnd, 0, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
    }
}