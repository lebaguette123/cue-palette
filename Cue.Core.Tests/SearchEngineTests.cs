using Cue.Core.Search;

namespace Cue.Core.Tests;

public class SearchEngineTests
{
    [Fact]
    public async Task Merges_providers_and_sorts_by_score_descending()
    {
        var engine = new SearchEngine([
            Provider("a", Result("a", "low", 10), Result("a", "high", 90)),
            Provider("b", Result("b", "mid", 50)),
        ]);

        var results = await engine.SearchAsync("x", CancellationToken.None);

        Assert.Equal(["high", "mid", "low"], results.Select(r => r.Title));
    }

    [Fact]
    public async Task Ties_are_broken_by_title_so_order_is_stable()
    {
        var engine = new SearchEngine([
            Provider("a", Result("a", "Banana", 5), Result("a", "apple", 5), Result("a", "Cherry", 5)),
        ]);

        var results = await engine.SearchAsync("x", CancellationToken.None);

        Assert.Equal(["apple", "Banana", "Cherry"], results.Select(r => r.Title));
    }

    [Fact]
    public async Task Returns_at_most_maxResults()
    {
        var engine = new SearchEngine(
            [Provider("a", Result("a", "1", 1), Result("a", "2", 2), Result("a", "3", 3))],
            maxResults: 2);

        var results = await engine.SearchAsync("x", CancellationToken.None);

        Assert.Equal(["3", "2"], results.Select(r => r.Title));
    }

    [Fact]
    public async Task A_failing_provider_does_not_hide_the_others()
    {
        var broken = new FakeProvider("broken", (_, _) => throw new InvalidOperationException("boom"));
        var engine = new SearchEngine([broken, Provider("ok", Result("ok", "fine", 1))]);

        var results = await engine.SearchAsync("x", CancellationToken.None);

        Assert.Equal(["fine"], results.Select(r => r.Title));
    }

    [Fact]
    public async Task Cancellation_is_not_swallowed()
    {
        var cancelling = new FakeProvider("slow", (_, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return [];
        });
        var engine = new SearchEngine([cancelling]);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => engine.SearchAsync("x", cts.Token));
    }

    private static SearchResult Result(string providerId, string title, double score)
        => new(providerId, title, title, null, score);

    private static FakeProvider Provider(string id, params SearchResult[] results)
        => new(id, (_, _) => results);

    private sealed class FakeProvider(
        string id,
        Func<string, CancellationToken, IReadOnlyList<SearchResult>> query) : ICommandProvider
    {
        public string Id => id;

        public Task<IReadOnlyList<SearchResult>> QueryAsync(string q, CancellationToken ct)
            => Task.FromResult(query(q, ct));

        public Task ExecuteAsync(SearchResult result, CancellationToken ct) => Task.CompletedTask;
    }
}