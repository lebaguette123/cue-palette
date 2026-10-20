using Cue.Core.Palette;
using Cue.Core.Search;

namespace Cue.Core.Tests;

public class PaletteViewModelTests
{
    [Fact]
    public async Task Search_fills_results_and_selects_the_first()
    {
        var vm = CreateViewModel(new StaticItem("notepad", "Notepad"), new StaticItem("calc", "Calculator"));

        await vm.SearchAsync("note");

        Assert.Equal(["Notepad"], vm.Results.Select(r => r.Title));
        Assert.Equal(0, vm.SelectedIndex);
    }

    [Fact]
    public async Task Search_with_no_matches_clears_results_and_selection()
    {
        var vm = CreateViewModel(new StaticItem("notepad", "Notepad"));
        await vm.SearchAsync("note");

        await vm.SearchAsync("zzz");

        Assert.Empty(vm.Results);
        Assert.Equal(-1, vm.SelectedIndex);
    }

    [Fact]
    public async Task A_slow_older_search_cannot_overwrite_a_newer_one()
    {
        var slow = new TaskCompletionSource<IReadOnlyList<SearchResult>>();
        var provider = new FakeProvider(q => q == "a"
            ? slow.Task
            : Task.FromResult<IReadOnlyList<SearchResult>>([Result("newer")]));
        var vm = new PaletteViewModel(new SearchEngine([provider]));

        var older = vm.SearchAsync("a");   // still waiting on its provider
        await vm.SearchAsync("b");         // the user typed again; this one finishes first
        slow.SetResult([Result("older")]); // the stale search finally completes
        await older;

        Assert.Equal(["newer"], vm.Results.Select(r => r.Title));
    }

    [Fact]
    public void SelectedIndex_raises_PropertyChanged()
    {
        var vm = CreateViewModel();

        Assert.PropertyChanged(vm, nameof(PaletteViewModel.SelectedIndex), () => vm.SelectedIndex = 3);
    }

    private static PaletteViewModel CreateViewModel(params StaticItem[] items)
        => new(new SearchEngine([new StaticProvider("sample", items)]));

    private static SearchResult Result(string title) => new("fake", title, title, null, 1);

    private sealed class FakeProvider(Func<string, Task<IReadOnlyList<SearchResult>>> query) : ICommandProvider
    {
        public string Id => "fake";

        public Task<IReadOnlyList<SearchResult>> QueryAsync(string q, CancellationToken ct) => query(q);

        public Task ExecuteAsync(SearchResult result, CancellationToken ct) => Task.CompletedTask;
    }
}