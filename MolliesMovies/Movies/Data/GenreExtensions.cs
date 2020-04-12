using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MolliesMovies.Common.Data;

namespace MolliesMovies.Movies.Data
{
    public static class GenreExtensions
    {
        public static async Task<ICollection<Genre>> AssertGenresAsync(
            this MolliesMoviesContext context, ICollection<string> toAssert, CancellationToken cancellationToken = default)
        {
            var cleaned = toAssert.Select(x => x.Trim()).ToList();
            var result = await context.Set<Genre>()
                .Where(x => cleaned.Contains(x.Name))
                .ToListAsync(cancellationToken);
            
            var newGenres = cleaned.Except(result.Select(x => x.Name)).ToList();
            if (newGenres.Any())
            {
                var toAdd = newGenres.Select(x => new Genre { Name = x }).ToList();
                
                result.AddRange(toAdd);
                await context.AddRangeAsync(toAdd, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
    }
}