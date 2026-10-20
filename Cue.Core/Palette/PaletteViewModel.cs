using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Cue.Core.Search;

namespace Cue.Core.Palette;

public sealed partial class PaletteViewModel : ObservableObject
{
    private readonly SearchEngine _engine;
    private CancellationTokenSource? _searchCts;
    
    public PaletteViewModel(SearchEngine engine)
    {
        _engine = engine;
        SelectedIndex = -1;
    }

    public ObservableCollection<SearchResult> Results { get; } = [];
    
    [ObservableProperty]
    public partial int SelectedIndex { get; set; }

    public async Task SearchAsync(string query)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            var results = await _engine.SearchAsync(query, token);

            if (token.IsCancellationRequested) return;

            Results.Clear();
            foreach (var result in results) Results.Add(result);
            SelectedIndex = Results.Count > 0 ? 0 : -1;
        }
        catch (OperationCanceledException)
        {
            // Nothing to show
        }
    }
}