namespace Cue.Core.Search;

public sealed record StaticItem(string Id, string Title, string? Subtitle = null);

public sealed class StaticProvider : ICommandProvider
{
    private readonly IReadOnlyList<StaticItem> _items;

    public StaticProvider(string id, IEnumerable<StaticItem> items)
    {
        Id = id;
        _items = [.. items];
    }
    
    public string Id { get; }

    public Task<IReadOnlyList<SearchResult>> QueryAsync(string query, CancellationToken cancellationToken)
    {
        var results = new List<SearchResult>();

        foreach (var item in _items)
        {
            if (FuzzyMatcher.Score(query, item.Title) is { } score) 
                results.Add(new SearchResult(Id, item.Id, item.Title, item.Subtitle, score));
        }
        
        return Task.FromResult<IReadOnlyList<SearchResult>>(results);
    }
    
    public Task ExecuteAsync(SearchResult result, CancellationToken cancellationToken) => Task.CompletedTask;
}