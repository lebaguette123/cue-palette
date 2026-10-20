using Windows.System;
using Cue.Core.Search;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Cue_App;

public sealed partial class PaletteView : UserControl
{
    public event EventHandler? EscapePressed;

    private static readonly SearchResult[] SampleResults =
    [
        new("sample", "notepad",   "Notepad",            "Application", 0),
        new("sample", "vscode",    "Visual Studio Code", "Application", 0),
        new("sample", "terminal",  "Windows Terminal",   "Application", 0),
        new("sample", "calc",      "Calculator",         "Application", 0),
        new("sample", "lock",      "Lock workstation",   "System command", 0),
        new("sample", "sleep",     "Sleep",              "System command", 0),
        new("sample", "downloads", "Open Downloads",     "Folder", 0),
        new("sample", "cue-repo",  "Open cue-palette",   "Project", 0),
    ];

    public PaletteView()
    {
        InitializeComponent();
        ResultList.ItemsSource = SampleResults;
        ResultList.SelectedIndex = 0;
        Loaded += (_, _) => FocusInput();
        KeyDown += OnKeyDown;
    }
    
    public void FocusInput()
    {
        SearchBox.Focus(FocusState.Programmatic);
        SearchBox.SelectAll();
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Escape) return;
        e.Handled = true;
        EscapePressed?.Invoke(this, EventArgs.Empty);
    }
}