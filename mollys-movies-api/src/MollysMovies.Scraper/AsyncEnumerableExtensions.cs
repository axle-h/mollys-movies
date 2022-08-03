using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MollysMovies.Scraper;

public static class AsyncEnumerableExtensions
{
    public static async Task<ICollection<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable,
        CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        await foreach (var t in enumerable.WithCancellation(cancellationToken))
        {
            list.Add(t);
        }

        return list;
    }

#pragma warning disable CS1998
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> data)
#pragma warning restore CS1998
    {
        foreach (var t in data)
        {
            yield return t;
        }
    }
}