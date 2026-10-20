using Cue.Core.Search;

namespace Cue.Core.Tests;

public class FuzzyMatcherTests
{
    [Theory]
    [InlineData("ntpd", "notepad")]
    [InlineData("NOTE", "notepad")]
    [InlineData("vsc", "Visual Studio Code")]
    [InlineData("", "anything")]
    public void Matches_when_query_is_a_case_insensitive_subsequence(string query, string candidate)
        => Assert.NotNull(FuzzyMatcher.Score(query, candidate));
    
    [Theory]
    [InlineData("dpn", "notepad")]      // right letters, wrong order
    [InlineData("xyz", "notepad")]      // letters absent
    [InlineData("noo", "notepad")]      // a candidate letter can't be used twice
    [InlineData("notepadd", "notepad")] // query longer than candidate
    public void Does_not_match_otherwise(string query, string candidate)
        => Assert.Null(FuzzyMatcher.Score(query, candidate));

    [Theory]
    [InlineData("note", "notepad", "nothing to eat")]     // consecutive beats scattered
    [InlineData("pad", "pad editor", "notepad")]          // start of a word beats mid-word
    [InlineData("vsc", "Visual Studio Code", "Overscan")] // word initials beat mid-word letters
    [InlineData("vsc", "VisualStudioCode", "Overscan")]   // camelCase humps count as word starts
    [InlineData("code", "Code", "Visual Studio Code")]    // less leading text ranks higher
    [InlineData("ab", "a-b", "a-----b")]
    public void Ranks_better_matches_higher(string query, string better, string worse)
        => Assert.True(Score(query, better) > Score(query, worse));
    
    private static int Score(string query, string candidate)
        => FuzzyMatcher.Score(query, candidate)
           ?? throw new InvalidOperationException($"'{query}' should match '{candidate}'");
}