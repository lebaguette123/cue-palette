using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cue_App;

public sealed partial class PaletteView : UserControl
{
    public event EventHandler? EscapePressed;

    public PaletteView()
    {
        InitializeComponent();
        Loaded += (_, _) => SearchBox.Focus(FocusState.Programmatic);
        SearchBox.KeyDown += (_, e) =>
        {
            if (e.Key != VirtualKey.Escape) return;
            e.Handled = true;
            EscapePressed?.Invoke(this, EventArgs.Empty);
        };
    }
}