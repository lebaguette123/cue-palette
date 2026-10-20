using Cue.Core.Search;

namespace Cue.Core.Tests;

public class SearchResultTests
{
    [Fact]
    public void Key_combines_provider_and_id()
    {
        var result = new SearchResult("apps", "notepad", "Notepad", null, 1.0);
        Assert.Equal("apps:notepad", result.Key);
    }
}