using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MollysMovies.Common.Scraper;
using MollysMovies.Scraper.Models;
using MollysMovies.Scraper.Plex.Models;

namespace MollysMovies.Scraper.Plex;

public class PlexScraper : ILocalScraper
{
    private readonly IPlexApiClient _client;
    private readonly ILogger<PlexScraper> _logger;
    private readonly IPlexMapper _mapper;

    public PlexScraper(IPlexApiClient client, ILogger<PlexScraper> logger, IPlexMapper mapper)
    {
        _client = client;
        _logger = logger;
        _mapper = mapper;
    }

    public string Source => "plex";

    public ScraperType Type => ScraperType.Local;

    public async Task<ScrapeImageResult> ScrapeImageAsync(string url,
        CancellationToken cancellationToken = default)
    {
        var (content, contentType) = await _client.GetThumbAsync(url, cancellationToken);
        return new ScrapeImageResult(content, contentType);
    }

    public async Task HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var movieLibraries = await _client.GetMovieLibrariesAsync(cancellationToken);
        if (!movieLibraries.Any())
        {
            throw new Exception("no movie libraries found on plex server");
        }
    }

    public async Task UpdateMovieLibrariesAsync(CancellationToken cancellationToken = default)
    {
        var libraries = await _client.GetMovieLibrariesAsync(cancellationToken);
        await Task.WhenAll(libraries.Select(x => _client.UpdateLibraryAsync(x.Key, cancellationToken)));
    }

    public async IAsyncEnumerable<CreateLocalMovieRequest> ScrapeMoviesAsync(ScrapeSession session,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var movie in EnumerateMoviesAsync(session.ScrapeFrom, cancellationToken))
        {
            yield return _mapper.ToCreateLocalMovieRequest(movie);
        }
    }

    private async IAsyncEnumerable<PlexMovie> EnumerateMoviesAsync(DateTime? scrapeFrom,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var libraries = await _client.GetMovieLibrariesAsync(cancellationToken);
        if (!libraries.Any())
        {
            _logger.LogWarning("no plex libraries found");
            yield break;
        }

        _logger.LogInformation("found {libraryCount} libraries", libraries.Count);

        var metadataLibraryTasks = libraries.Select(x => _client.GetAllMovieMetadataAsync(x.Key, cancellationToken));
        var allMetadata = (await Task.WhenAll(metadataLibraryTasks)).SelectMany(x => x).ToList();
        var toScrape = scrapeFrom.HasValue
            ? allMetadata.Where(x => x.DateCreated > scrapeFrom.Value).ToList()
            : allMetadata;

        if (!toScrape.Any())
        {
            yield break;
        }

        const int pageSize = 50;
        var pages = toScrape
            .Select((meta, index) => (meta, index))
            .GroupBy(x => x.index / pageSize)
            .Select(grp => grp.Select(x => x.meta));

        foreach (var page in pages)
        {
            var tasks = page.Select(x => _client.GetMovieAsync(x.RatingKey, cancellationToken));
            var movies = await Task.WhenAll(tasks);
            foreach (var movie in movies)
            {
                if (movie is not null)
                {
                    yield return movie;
                }
            }
        }
    }
}