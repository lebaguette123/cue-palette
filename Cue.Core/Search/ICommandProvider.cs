namespace Cue.Core.Search;

public interface ICommandProvider
{
    string Id { get; }
    Task<IReadOnlyList<SearchResult>> QueryAsync(string query, CancellationToken cancellationToken);
    
    Task ExecuteAsync(SearchResult result, CancellationToken cancellationToken);
    
}