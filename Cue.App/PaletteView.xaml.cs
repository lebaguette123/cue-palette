using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Cue_App;

public sealed partial class PaletteView : UserControl
{
    public event EventHandler? EscapePressed;

    public PaletteView()
    {
        InitializeComponent();
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