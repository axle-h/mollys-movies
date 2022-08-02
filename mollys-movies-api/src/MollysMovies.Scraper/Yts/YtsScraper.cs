using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MollysMovies.Common.Scraper;
using MollysMovies.Scraper.Models;
using MollysMovies.Scraper.Yts.Models;

namespace MollysMovies.Scraper.Yts;

public class YtsScraper : ITorrentScraper
{
    private readonly IYtsClient _client;
    private readonly ILogger<YtsScraper> _logger;
    private readonly IYtsMapper _mapper;
    private readonly ScraperOptions _options;

    public YtsScraper(
        IYtsClient client,
        ILogger<YtsScraper> logger,
        IOptions<ScraperOptions> options,
        IYtsMapper mapper)
    {
        _client = client;
        _logger = logger;
        _options = options.Value;
        _mapper = mapper;
    }

    public string Source => "yts";

    public ScraperType Type => ScraperType.Torrent;

    public async Task<ScrapeImageResult> ScrapeImageAsync(string url, CancellationToken cancellationToken = default)
    {
        var (content, contentType) = await _client.GetImageAsync(url, cancellationToken);
        return new ScrapeImageResult(content, contentType);
    }

    public Task HealthCheckAsync(CancellationToken cancellationToken = default) =>
        _client.HealthCheckAsync(cancellationToken);

    public async IAsyncEnumerable<CreateMovieRequest> ScrapeMoviesAsync(ScrapeSession session,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var limit = _options.Yts?.Limit ?? 0;
        if (limit <= 0)
        {
            throw new Exception("configured YTS limit must be > 0");
        }

        _logger.LogInformation("Scraping yts movies from {FromDate}", session.ScrapeFrom);

        for (var page = 1; page < int.MaxValue; page++)
        {
            _logger.LogInformation("scraping page {Page}", page);

            var response = await _client.ListMoviesAsync(new YtsListMoviesRequest
            {
                Page = page,
                Limit = limit,
                OrderBy = "desc",
                SortBy = "date_added"
            }, cancellationToken);

            if (response.Movies is null || !response.Movies.Any())
            {
                _logger.LogInformation("no movies on page {Page}, ending scrape session", page);
                break;
            }

            // Select movies newer than the newest, previously scraped movie.
            var movies = session.ScrapeFrom.HasValue
                ? response.Movies
                    .Where(x => x.DateUploaded > session.ScrapeFrom.Value)
                    .ToList()
                : response.Movies;

            if (!movies.Any())
            {
                _logger.LogInformation(
                    "all movies on page {Page} are older than the latest scraped movie, ending scrape session", page);
                break;
            }

            _logger.LogInformation("scraped {MovieCount} movies", movies.Count);

            var requests = movies.Select(_mapper.ToCreateMovieRequest).ToList();

            foreach (var request in requests.OrderBy(x => x.DateCreated))
            {
                // yield ordered by date created so we dont miss any on error when filtering by scrape date
                yield return request;
            }

            _logger.LogInformation("added {MovieCount} movies", requests.Count);

            if (movies.Count < limit)
            {
                _logger.LogInformation(
                    "only scraped {Count} movies on page {Page} with limit {Limit}, ending scrape session",
                    movies.Count,
                    page, limit);
                break;
            }

            if (_options.RemoteScrapeDelay > TimeSpan.Zero)
            {
                await Task.Delay(_options.RemoteScrapeDelay, cancellationToken);
            }
        }

        _logger.LogInformation("done");
    }
}