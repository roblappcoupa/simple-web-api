namespace WebApi.Utils;

using WebApi.Models;

internal static class CollectionExtensions
{
    public static List<WordCount> TopN(this Dictionary<string, long> data, int n)
        => data
            .Select(x => new WordCount(x.Key, x.Value))
            .OrderByDescending(x => x.Frequency)
            .Take(n)
            .ToList();

}