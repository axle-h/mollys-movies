using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MolliesMovies.Common.Data;

namespace MolliesMovies.Movies.Data
{
    public static class LocalMovieExtensions
    {
        public static async Task<LocalMovie> GetLatestLocalMovieBySourceAsync(this MolliesMoviesContext context,
            string source,
            CancellationToken cancellationToken = default) =>
            await context.Set<LocalMovie>()
                .Where(x => x.Source == source)
                .OrderByDescending(x => x.DateCreated)
                .FirstOrDefaultAsync(cancellationToken);
    }
}