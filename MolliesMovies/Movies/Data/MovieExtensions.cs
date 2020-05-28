using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MolliesMovies.Common.Data;

namespace MolliesMovies.Movies.Data
{
    public static class MovieExtensions
    {
        public static IQueryable<Movie> Movies(this MolliesMoviesContext context) =>
            context.Set<Movie>()
                .AsNoTracking()
                .Include(x => x.MovieGenres)
                .ThenInclude(x => x.Genre)
                .Include(x => x.MovieSources)
                .ThenInclude(x => x.Torrents)
                .Include(x => x.DownloadedMovies)
                .ThenInclude(x => x.LocalMovie)
                .Include(x => x.TransmissionContexts)
                .ThenInclude(x => x.Statuses);
        
        public static async Task<Movie> GetMovieDetailByIdAsync(this MolliesMoviesContext context, int id, CancellationToken cancellationToken = default) =>
            await context.Movies().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        public static async Task<Movie> GetMovieByImdbCodeAsync(this MolliesMoviesContext context, string imdbCode, CancellationToken cancellationToken = default) =>
            await context.Set<Movie>().FirstOrDefaultAsync(x => x.ImdbCode == imdbCode, cancellationToken);

        public static async Task<MovieSource> GetLatestMovieBySourceAsync(this MolliesMoviesContext context,
            string source,
            CancellationToken cancellationToken = default) =>
            await context.Set<MovieSource>()
                .Where(x => x.Source == source)
                .OrderByDescending(x => x.DateCreated)
                .FirstOrDefaultAsync(cancellationToken);

        public static async Task<PaginatedData<Movie>> SearchMoviesAsync(this MolliesMoviesContext context, PaginatedMovieQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = context.Movies();

            if (!(query.Title is null))
            {
                var pattern = $"%{query.Title}%";
                dbQuery = dbQuery.Where(m => EF.Functions.Like(m.Title, pattern));
            }

            if (!(query.Quality is null))
            {
                dbQuery = dbQuery.Where(m => m.MovieSources.Any(s => s.Torrents.Any(t => t.Quality == query.Quality)));
            }
            
            if (!(query.Language is null))
            {
                dbQuery = dbQuery.Where(m => m.Language == query.Language);
            }

            if (query.Downloaded.HasValue)
            {
                dbQuery = query.Downloaded.Value
                    ? dbQuery.Where(x => x.DownloadedMovies.Any())
                    : dbQuery.Where(x => !x.DownloadedMovies.Any());
            }

            if (!string.IsNullOrEmpty(query.Genre))
            {
                dbQuery = dbQuery.Where(x => x.MovieGenres.Any(g => g.Genre.Name == query.Genre));
            }

            if (!(query.OrderBy is null))
            {
                for (var i = 0; i < query.OrderBy.Count; i++)
                {
                    var by = query.OrderBy.ElementAt(i);
                    dbQuery = (i == 0, @by.Descending) switch
                    {
                        (true, false) => dbQuery.OrderBy(@by.Property),
                        (true, true) => dbQuery.OrderByDescending(@by.Property),
                        (false, false) => ((IOrderedQueryable<Movie>) dbQuery).ThenBy(@by.Property),
                        (false, true) => ((IOrderedQueryable<Movie>) dbQuery).ThenByDescending(@by.Property)
                    };
                }
            }

            var count = await dbQuery.CountAsync(cancellationToken);
            var movies = count > 0
                ? await dbQuery
                    .Skip((query.Page - 1) * query.Limit)
                    .Take(query.Limit)
                    .ToListAsync(cancellationToken)
                : new List<Movie>();
            
            return new PaginatedData<Movie>
            {
                Page = query.Page,
                Limit = query.Limit,
                Count = count,
                Data = movies
            };
        }
    }
}