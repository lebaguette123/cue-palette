namespace Cue.Core.Search;

public static class FuzzyMatcher
{
    private const int MatchScore = 10;
    private const int BoundaryBonus = 20;
    private const int ConsecutiveBonus = 15;
    private const int GapPenalty = 3;
    private const int LeadingPenalty = 1;
    private const int Unreachable = int.MinValue / 2;

    public static int? Score(string query, string candidate)
    {
        if (query.Length == 0) return 0;
        if (query.Length > candidate.Length) return null;

        var previous = new int[candidate.Length];
        var current = new int[candidate.Length];

        for (var i = 0; i < query.Length; i++)
        {
            var queryChar = char.ToLowerInvariant(query[i]);
            var skipBest = Unreachable;

            for (var j = 0; j < candidate.Length; j++)
            {
                current[j] = Unreachable;
                if (char.ToLowerInvariant(candidate[j]) == queryChar)
                {
                    var best = i == 0 ? -LeadingPenalty * j 
                        : Math.Max(j > 0 ? previous[j-1] + ConsecutiveBonus : Unreachable, skipBest);
                    
                    if (best > Unreachable / 2) current[j] = best + MatchScore + (IsBoundary(candidate, j) ? BoundaryBonus : 0);
                }

                if (i > 0 && j > 0) skipBest = Math.Max(skipBest, previous[j - 1]) - GapPenalty;
            }
            (previous, current) = (current, previous);
        }

        var result = previous.Max();
        return result > Unreachable / 2 ? result : null;
    }

    private static bool IsBoundary(string text, int index)
    {
        if (index == 0) return true;
        var before = text[index - 1];
        if (!char.IsLetterOrDigit(before)) return true;
        return char.IsLower(before) && char.IsUpper(text[index]);
    }
}