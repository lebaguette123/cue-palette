namespace Cue.Core.Search;

public sealed record SearchResult(
    string ProviderId,
    string Id,
    string Title,
    string? Subtitle,
    double Score)
{
    public string Key => $"{ProviderId}:{Id}";
}