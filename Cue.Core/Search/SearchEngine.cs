namespace Cue.Core.Search;

public sealed class SearchEngine
{
    private readonly IReadOnlyList<ICommandProvider> _providers;
    private readonly int _maxResults;

    public SearchEngine(IEnumerable<ICommandProvider> providers, int maxResults = 8)
    {
        _providers = [.. providers];
        _maxResults = maxResults;
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var batches = await Task.WhenAll(
            _providers.Select(p => QuerySafelyAsync(p, query, cancellationToken)));
        return
        [
            .. batches
                .SelectMany(batch => batch)
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Title, StringComparer.OrdinalIgnoreCase)
                .Take(_maxResults)
        ];
    }

    private static async Task<IReadOnlyList<SearchResult>> QuerySafelyAsync(
        ICommandProvider provider, string query, CancellationToken cancellationToken)
    {
        try
        {
            return await provider.QueryAsync(query, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return [];
        }
    }
}