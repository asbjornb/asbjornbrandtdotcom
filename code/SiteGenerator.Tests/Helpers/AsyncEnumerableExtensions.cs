namespace SiteGenerator.Tests.Helpers;

public static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
    {
        var list = new List<T>();
        await foreach (var item in asyncEnumerable)
        {
            list.Add(item);
        }
        return list;
    }

    public static IAsyncEnumerable<T> ToIAsyncEnumerable<T>(this List<T> list)
    {
        return GetAsyncEnumerable(list);
    }

    private static async IAsyncEnumerable<T> GetAsyncEnumerable<T>(List<T> list)
    {
        foreach (var item in list)
        {
            await Task.Yield();
            yield return item;
        }
    }
}
