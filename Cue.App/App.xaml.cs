using Cue.Core.Hotkeys;
using Cue.Windows;
using Cue.Core.Palette;
using Cue.Core.Search;
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
        _window = new MainWindow(CreatePalette());
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

    private static PaletteViewModel CreatePalette()
    {
        StaticItem[] items =
        [
            new("notepad",   "Notepad",            "Application"),
            new("vscode",    "Visual Studio Code", "Application"),
            new("terminal",  "Windows Terminal",   "Application"),
            new("calc",      "Calculator",         "Application"),
            new("lock",      "Lock workstation",   "System command"),
            new("sleep",     "Sleep",              "System command"),
            new("downloads", "Open Downloads",     "Folder"),
            new("cue-repo",  "Open cue-palette",   "Project"),
        ];

        var engine = new SearchEngine([new StaticProvider("sample", items)]);
        return new PaletteViewModel(engine);
    }
}