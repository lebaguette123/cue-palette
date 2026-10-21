using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using Cue.Windows;
using Cue.Core.Palette;
using WinRT.Interop;

namespace Cue_App;

public sealed partial class MainWindow : Window
{
    private const int WidthDip = 640;
    private const int HeightDip = 400;

    public MainWindow(PaletteViewModel viewModel)
    {
        InitializeComponent();
        Palette.ViewModel = viewModel;
        ConfigureWindow();
        Palette.EscapePressed += (_, _) => HidePalette();   // temporary until the hotkey exists
        Activated += OnActivated;
    }

    public void ShowPalette()
    {
        Palette.ResetForShow();
        PositionOnCursorMonitor();
        AppWindow.Show();
        Activate();
        WindowFocus.BringToForeground(WindowNative.GetWindowHandle(this));
        Palette.FocusInput();
    }

    public void HidePalette() => AppWindow.Hide();

    public void TogglePalette()
    {
        if(AppWindow.IsVisible) HidePalette();
        else ShowPalette();
    }

    private void OnActivated(object sender, WindowActivatedEventArgs e)
    {
        if (e.WindowActivationState == WindowActivationState.Deactivated) HidePalette();
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
        
        WindowChrome.StripFrame(hwnd);
        WindowChrome.UseSquareCorners(hwnd);
    }

    private void PositionOnCursorMonitor()
    {
        ResizePalette(HeightDip);
    }

    private void ResizePalette(int heightDip)
    {
        var (cursorX, cursorY) = Monitors.GetCursorPosition();
        
        var area = DisplayArea.GetFromPoint(
            new PointInt32(cursorX, cursorY), 
            DisplayAreaFallback.Nearest).WorkArea;
        
        var scale = Monitors.GetScaleAt(cursorX, cursorY);
        var width = (int)(WidthDip * scale);
        var height = (int)(heightDip * scale);

        var x = area.X + (area.Width - width) / 2;
        var y = area.Y + (int)(area.Height * 0.2);
        
        AppWindow.MoveAndResize(new RectInt32(x, y, width, height));
    }
}