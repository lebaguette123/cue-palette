using Cue.Core.Hotkeys;
using Cue.Windows;
using Microsoft.UI.Xaml;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Cue_App;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private MainWindow? _window;
    private IHotkeyService? _hotkeys;
    private TrayIcon? _tray;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        var hwnd = WindowNative.GetWindowHandle(_window);
        
        _hotkeys = new HotkeyService(hwnd);
        _hotkeys.Pressed += _window.TogglePalette;
        
        _tray = new TrayIcon(hwnd, "Cue");
        _tray.Clicked += _window.ShowPalette;
        _tray.QuitRequested += Quit;

        _window.Closed += (_, _) =>
        {
            _tray?.Dispose();
            _hotkeys?.Dispose();
        };

        if (!_hotkeys.Register(HotkeyBinding.Default)) _window.ShowPalette();
    }

    private void Quit()
    {
        _tray?.Dispose();
        _hotkeys?.Dispose();
        Exit();
    }
}