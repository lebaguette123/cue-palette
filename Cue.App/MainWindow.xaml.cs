using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinRT.Interop;

namespace Cue_App;

public sealed partial class MainWindow : Window
{
    private const int WidthDip = 640;
    private const int HeightDip = 400;

    public MainWindow()
    {
        InitializeComponent();
        ConfigureWindow();
        Palette.EscapePressed += (_, _) => Close();   // temporary until the hotkey exists
    }

    private void ConfigureWindow()
    {
        var hwnd = WindowNative.GetWindowHandle(this);

        var presenter = (OverlappedPresenter)AppWindow.Presenter;
        presenter.SetBorderAndTitleBar(false, false);
        presenter.IsAlwaysOnTop = true;
        presenter.IsResizable = false;
        presenter.IsMaximizable = false;
        presenter.IsMinimizable = false;
        AppWindow.IsShownInSwitchers = false;
        StripFrame(hwnd);

        // Square corners instead of Windows 11's default rounding.
        var doNotRound = 1; // DWMWCP_DONOTROUND
        DwmSetWindowAttribute(hwnd, 33 /* DWMWA_WINDOW_CORNER_PREFERENCE */, ref doNotRound, sizeof(int));
        
        // DIPs -> physical pixels
        var scale = GetDpiForWindow(hwnd) / 96.0;
        var width = (int)(WidthDip * scale);
        var height = (int)(HeightDip * scale);

        // Horizontally centered on the work area, about a fifth of the way down.
        var area = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary).WorkArea;
        var x = area.X + (area.Width - width) / 2;
        var y = area.Y + (int)(area.Height * 0.2);
        AppWindow.MoveAndResize(new RectInt32(x, y, width, height));
    }
    
    private static void StripFrame(IntPtr hwnd)
    {
        const int GWL_STYLE = -16, GWL_EXSTYLE = -20;
        const nint WS_CAPTION = 0x00C00000;        // title bar + border + dialog frame
        const nint WS_THICKFRAME = 0x00040000;     // resizable frame
        const nint WS_EX_DLGMODALFRAME = 0x00000001;
        const nint WS_EX_WINDOWEDGE = 0x00000100;  // the raised 3D edge
        const nint WS_EX_CLIENTEDGE = 0x00000200;
        const nint WS_EX_STATICEDGE = 0x00020000;
        const uint SWP_NOSIZE = 0x1, SWP_NOMOVE = 0x2, SWP_NOZORDER = 0x4,
            SWP_NOACTIVATE = 0x10, SWP_FRAMECHANGED = 0x20;

        var style = GetWindowLongPtr(hwnd, GWL_STYLE);
        SetWindowLongPtr(hwnd, GWL_STYLE, style & ~(WS_CAPTION | WS_THICKFRAME));

        var ex = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
        SetWindowLongPtr(hwnd, GWL_EXSTYLE,
            ex & ~(WS_EX_DLGMODALFRAME | WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE));

        // Tell Windows to redraw the frame with the new styles.
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
    
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr(IntPtr hwnd, int index, nint value);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);
}