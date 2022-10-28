using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MollysMovies.Common;
using MollysMovies.Common.Scraper;
using MollysMovies.Scraper.Movies;
using MollysMovies.ScraperClient;

namespace MollysMovies.Scraper;

public interface IScraperService
{
    Task<NotifyScrapeComplete> ScrapeAsync(string id, CancellationToken cancellationToken = default);

    Task UpdateLocalMovieLibrariesAsync(CancellationToken cancellationToken = default);

    Task<bool> ScrapeForLocalMovieAsync(string imdbCode, CancellationToken cancellationToken = default);
}

public class ScraperService : IScraperService
{
    private readonly ISystemClock _clock;
    private readonly ILogger<ScraperService> _logger;
    private readonly IMovieService _movieService;
    private readonly IScrapeRepository _repository;
    private readonly IScraperClient _scraperClient;
    private readonly ICollection<IScraper> _scrapers;

    public ScraperService(
        IEnumerable<IScraper> scrapers,
        ISystemClock clock,
        ILogger<ScraperService> logger,
        IMovieService movieService,
        IScrapeRepository repository,
        IScraperClient scraperClient)
    {
        _clock = clock;
        _logger = logger;
        _movieService = movieService;
        _repository = repository;
        _scraperClient = scraperClient;
        _scrapers = scrapers.ToList();
    }

    public async Task<NotifyScrapeComplete> ScrapeAsync(string id, CancellationToken cancellationToken = default)
    {
        var scrape = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new Exception($"cannot find Scrape with id {id}");

        foreach (var scraper in _scrapers)
        {
            var source = new ScrapeSource
            {
                Source = scraper.Source,
                Type = scraper.Type,
                StartDate = _clock.UtcNow
            };
            scrape.Sources.Add(source);

            try
            {
                var session = await _movieService.GetScrapeSessionAsync(scraper.Source, scraper.Type, cancellationToken);
                switch (scraper)
                {
                    case ILocalScraper localScraper:
                        await foreach (var movie in localScraper.ScrapeMoviesAsync(session, cancellationToken))
                        {
                            scrape.LocalMovieCount++;
                            source.MovieCount++;
                            await _movieService.CreateLocalMovieAsync(session, movie, cancellationToken);
                        }
                        break;

                    case ITorrentScraper torrentScraper:
                        await foreach (var movie in torrentScraper.ScrapeMoviesAsync(session, cancellationToken))
                        {
                            try
                            {
                                await _movieService.CreateMovieAsync(session, movie, cancellationToken);
                            }
                            catch (ValidationException e)
                            {
                                _logger.LogError(e, "invalid movie from {Source} with imdb {ImdbCode}", scraper.Source, movie.ImdbCode);
                                continue;
                            }
                            
                            scrape.MovieCount++;
                            scrape.TorrentCount += movie.Torrents.Count;

                            source.MovieCount++;
                            source.TorrentCount += movie.Torrents.Count;
                            await _scraperClient.ScrapeMovieImageAsync(movie.ImdbCode, scraper.Source, movie.SourceCoverImageUrl, cancellationToken);
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(scraper.GetType().ToString(), "unknown scraper type");
                }

                source.Success = true;
                source.EndDate = _clock.UtcNow;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "failed to scrape {Source}", scraper.Source);
                source.Success = false;
                source.EndDate = _clock.UtcNow;
                source.Error = e.ToString();
            }

            await _repository.ReplaceAsync(scrape, cancellationToken);
        }

        var errors = scrape.Sources
            .Where(x => !string.IsNullOrEmpty(x.Error))
            .Select(x => new ScrapeSourceFailure(x.Source!, x.Type.ToString(), x.Error!))
            .ToList();
        scrape.Success = !errors.Any();
        scrape.EndDate = _clock.UtcNow;
        await _repository.ReplaceAsync(scrape, cancellationToken);
        return new NotifyScrapeComplete(id, errors);
    }

    public async Task UpdateLocalMovieLibrariesAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _scrapers
            .OfType<ILocalScraper>()
            .Select(x => x.UpdateMovieLibrariesAsync(cancellationToken))
            .ToList();
        await Task.WhenAll(tasks);
    }

    public async Task<bool> ScrapeForLocalMovieAsync(string imdbCode, CancellationToken cancellationToken = default)
    {
        foreach (var scraper in _scrapers.OfType<ILocalScraper>())
        {
            var session = await _movieService.GetScrapeSessionAsync(scraper.Source, scraper.Type, cancellationToken);

            var result = false;
            await foreach (var scrapedMovie in scraper.ScrapeMoviesAsync(session, cancellationToken))
            {
                await _movieService.CreateLocalMovieAsync(session, scrapedMovie, cancellationToken);
                if (scrapedMovie.ImdbCode == imdbCode)
                {
                    result = true;
                }
            }

            return result;
        }

        return false;
    }
}