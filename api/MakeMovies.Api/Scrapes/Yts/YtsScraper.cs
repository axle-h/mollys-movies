using System.Runtime.CompilerServices;
using MakeMovies.Api.Movies;
using Microsoft.Extensions.Options;

namespace MakeMovies.Api.Scrapes.Yts;

public class YtsScraper(IYtsClient client, ILogger<YtsScraper> logger, IOptions<ScrapeOptions> options) : IScraper
{
    public async IAsyncEnumerable<Movie> ScrapeMoviesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var limit = options.Value.Yts?.Limit ?? 0;
        if (limit <= 0)
        {
            throw new Exception("configured YTS limit must be > 0");
        }

        logger.LogInformation("Scraping yts movies");

        for (var page = 1; page < int.MaxValue; page++)
        {
            logger.LogDebug("scraping page {Page} begin", page);

            var response = await client.ListMoviesAsync(page, limit, cancellationToken);
            var movies = response.Movies;
            if (movies is null || movies.Count == 0)
            {
                logger.LogInformation("no movies on page {Page}, ending scrape session", page);
                break;
            }

            logger.LogInformation("scraped {MovieCount} movies from page {Page}", movies.Count, page);

            foreach (var movie in movies)
            {
                yield return ToMovie(movie);
            }

            logger.LogDebug("scraping page {Page} end", page);
        }
    }
    
    private static Torrent ToTorrent(YtsTorrent torrent) =>
        new(
            torrent.Hash,
            torrent.Quality,
            torrent.Type,
            torrent.SizeBytes,
            DateTimeOffset.FromUnixTimeSeconds(torrent.DateUploadedUnix).UtcDateTime
        );

    private static Movie ToMovie(YtsMovie movie)
    {
        var title = movie.TitleEnglish ?? movie.Title;
        return new Movie(
            $"yts_{movie.Id}",
            movie.ImdbCode,
            title,
            MovieExtensions.CleanTitle(title),
            NullIfEmpty(movie.Language) ?? "unknown",
            movie.Year,
            movie.Rating,
            TimeSpan.FromMinutes(movie.Runtime),
            NullIfEmpty(movie.DescriptionFull) ?? NullIfEmpty(movie.Summary) ?? NullIfEmpty(movie.Synopsis),
            movie.Genres?.Select(MovieExtensions.CleanTitle).Where(g => g.Length > 0).ToHashSet() ?? [],
            NullIfEmpty(movie.YtTrailerCode),
            DateTimeOffset.FromUnixTimeSeconds(movie.DateUploadedUnix).UtcDateTime,
            movie.Torrents?.Select(ToTorrent).ToList() ?? []
        );
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrEmpty(s) ? null : s;
}