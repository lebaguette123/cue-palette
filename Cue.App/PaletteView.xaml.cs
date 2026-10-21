using Windows.System;
using Cue.Core.Palette;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Cue_App;

public sealed partial class PaletteView : UserControl
{
    public event EventHandler? EscapePressed;
    private bool _suppressSearchTextChanged;
    public PaletteViewModel? ViewModel
    {
        get;
        set
        {
            field = value;
            Bindings.Update();
        }
    }

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

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressSearchTextChanged) return;
        var viewModel = ViewModel ?? throw new InvalidOperationException("PaletteView.ViewModel was not set");
        await viewModel.SearchAsync(SearchBox.Text);
    }

    public void ResetForShow()
    {
        _suppressSearchTextChanged = true;

        try
        {
            SearchBox.Text = string.Empty;
        }
        finally
        {
            _suppressSearchTextChanged = false;
        }

        var viewModel = ViewModel ?? throw new InvalidOperationException("PaletteView.ViewModel was not set");
        _ = viewModel.SearchAsync(string.Empty);
    }
}