using System.Globalization;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FuzzySharp;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MakeMovies.Api.Movies;

public record ScrapeStats(int Movies, int Torrents);

public interface IMovieRepository : IHealthCheck
{
    Task UpdateLibraryMoviesAsync(ISet<string> imdbCodesInLibrary, CancellationToken cancellationToken = default);
    
    Task<ScrapeStats> WriteScrapedAsync(IAsyncEnumerable<Movie> movies, CancellationToken cancellationToken = default);

    Task<PaginatedData<MovieSummary>> ListAsync(PaginatedQuery<SourceMovieField> query, CancellationToken cancellationToken = default);

    Task<ISet<string>> GetAllGenresAsync(CancellationToken cancellationToken = default);
    
    Task<Movie?> GetAsync(string id, CancellationToken cancellationToken = default);
}

public class InMemoryMovieRepository(Db db) : IMovieRepository
{

    private ScrapeStats Stats()
    {
        var torrents = db.Movies.AsEnumerable().Sum(movie => movie.Torrents.Count);
        return new ScrapeStats(db.Movies.Count, torrents);
    }
    
    public async Task UpdateLibraryMoviesAsync(ISet<string> imdbCodesInLibrary, CancellationToken cancellationToken = default)
    {
        foreach (var movie in db.Movies.AsEnumerable())
        {
            var inLibrary = imdbCodesInLibrary.Contains(movie.ImdbCode);
            if (inLibrary != movie.InLibrary)
            {
                await db.Movies.UpsertAsync(movie.Id, movie with { InLibrary = true }, cancellationToken);
            }
        }
    }

    public async Task<ScrapeStats> WriteScrapedAsync(IAsyncEnumerable<Movie> movies, CancellationToken cancellationToken = default)
    {
        await foreach (var movie in movies.WithCancellation(cancellationToken))
        {
            await db.Movies.UpsertAsync(movie.Id, movie, cancellationToken);
        }
        return Stats();
    }

    public Task<PaginatedData<MovieSummary>> ListAsync(PaginatedQuery<SourceMovieField> query, CancellationToken cancellationToken = default)
    {
        
        var sortedMovies = db.Movies.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = MovieExtensions.CleanTitle(query.Search);
            sortedMovies = sortedMovies
                .OrderByDescending(movie => Fuzz.WeightedRatio(search, movie.SearchableTitle));
        }
        else
        {
            var orderBy = query.OrderBy;
            var descending = query.Descending;
            if (orderBy == default)
            {
                orderBy = SourceMovieField.DateCreated;
                descending = true;
            }

            Func<Movie, object> lambda = orderBy switch
            {
                SourceMovieField.Title => m => m.Title,
                SourceMovieField.DateCreated => m => m.DateCreated,
                SourceMovieField.Year => m => m.Year,
                _ => throw new ArgumentOutOfRangeException(nameof(query), "unknown order by")
            };
            
            var preSortedMovies = descending
                ? sortedMovies.OrderByDescending(lambda)
                : sortedMovies.OrderBy(lambda);

            if (orderBy == SourceMovieField.Year)
            {
                // ordering by year is a bit crap since we dont have the entire movie date
                // to simulate something useful also order by the date created
                preSortedMovies = descending
                    ? preSortedMovies.ThenByDescending(m => m.DateCreated)
                    : preSortedMovies.ThenBy(m => m.DateCreated);
            }

            sortedMovies = preSortedMovies;
        }

        var movies = sortedMovies
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .Select(m => m.Summary())
            .ToList();

        var result = new PaginatedData<MovieSummary>(query.Page, query.Limit, db.Movies.Count, movies);
        return Task.FromResult(result);
    }

    public Task<ISet<string>> GetAllGenresAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(db.Movies.AsQueryable().SelectMany(m => m.Genres).ToHashSet() as ISet<string>);

    public Task<Movie?> GetAsync(string id, CancellationToken cancellationToken = default) =>
        Task.FromResult(db.Movies.Get(id));

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(HealthCheckResult.Healthy());
    
    
}
