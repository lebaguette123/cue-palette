using Cue.Core.Search;

namespace Cue.Core.Tests;

public class StaticProviderTests
{
    private static readonly StaticItem[] Items =
    [
        new("notepad", "Notepad", "Application"),
        new("calc", "Calculator", "Application"),
        new("lock", "Lock workstation", "System command"),
    ];

    [Fact]
    public async Task Returns_only_items_the_query_matches()
    {
        var provider = new StaticProvider("sample", Items);

        var results = await provider.QueryAsync("note", CancellationToken.None);

        var result = Assert.Single(results);
        Assert.Equal("sample:notepad", result.Key);
        Assert.Equal("Notepad", result.Title);
        Assert.Equal("Application", result.Subtitle);
    }

    [Fact]
    public async Task Empty_query_returns_every_item()
    {
        var provider = new StaticProvider("sample", Items);

        var results = await provider.QueryAsync("", CancellationToken.None);

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task Works_with_the_engine_to_rank_word_initials_first()
    {
        var provider = new StaticProvider("sample",
        [
            new StaticItem("overscan", "Overscan"),
            new StaticItem("vscode", "Visual Studio Code"),
        ]);
        var engine = new SearchEngine([provider]);

        var results = await engine.SearchAsync("vsc", CancellationToken.None);

        Assert.Equal(["Visual Studio Code", "Overscan"], results.Select(r => r.Title));
    }
}