using System.Runtime.CompilerServices;
using MakeMovies.Api.Movies;
using Microsoft.Extensions.Options;

namespace MakeMovies.Api.Scrapes;

public interface IRootScraper
{
    IAsyncEnumerable<Movie> ScrapeMoviesAsync(CancellationToken cancellationToken = default);
}

public class RootScraper(IEnumerable<IScraper> scrapers, ILogger<RootScraper> logger, IOptions<ScrapeOptions> options) : IRootScraper
{
    private readonly List<IScraper> _scrapers = scrapers.ToList();

    public async IAsyncEnumerable<Movie> ScrapeMoviesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var languages = options.Value.Languages;
        foreach (var scraper in _scrapers)
        {
            await foreach (var movie in scraper.ScrapeMoviesAsync(cancellationToken))
            {
                if (string.IsNullOrWhiteSpace(movie.Id)
                    || string.IsNullOrWhiteSpace(movie.ImdbCode)
                    || string.IsNullOrWhiteSpace(movie.Title)
                    || movie.Year < 1900 || movie.Year > 2100
                    || movie.Rating < 0)
                {
                    logger.LogDebug("Discarding bad movie: id={Id} imdb={ImdbCode}", movie.Id, movie.ImdbCode);
                    continue;
                }

                if (!languages.Contains(movie.Language))
                {
                    logger.LogDebug("Discarding foreign language movie: title={Title} language={Language}", movie.Title, movie.Language);
                    continue;
                }
                
                for (var i = movie.Torrents.Count - 1; i >= 0; i--)
                {
                    var torrent = movie.Torrents[i];
                    if (string.IsNullOrWhiteSpace(torrent.Hash)
                        || string.IsNullOrWhiteSpace(torrent.Quality)
                        || string.IsNullOrWhiteSpace(torrent.Type)
                        // 3d was a fad
                        || torrent.Type == "3D" || torrent.Type == "3d")
                    {
                        logger.LogDebug(
                            "Discarding bad torrent: movie id={Id} imdb={ImdbCode} quality={Quality} type={Type}",
                            movie.Id, movie.ImdbCode, torrent.Quality, torrent.Type);
                        movie.Torrents.RemoveAt(i);
                    }
                }

                if (movie.Torrents.Count == 0)
                {
                    logger.LogDebug("Discarding movie with no good torrents: id={Id} imdb={ImdbCode}", movie.Id, movie.ImdbCode);
                    continue;
                }
                
                yield return movie;
            }
        }
    }
}