using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MollysMovies.Common;
using MollysMovies.Common.Movies;
using MollysMovies.Common.Scraper;
using MollysMovies.Scraper.Models;

namespace MollysMovies.Scraper.Movies;

public interface IMovieService
{
    Task SetStatusAsync(string imdbCode, MovieDownloadStatusCode status,
        CancellationToken cancellationToken = default);

    Task<ScrapeSession> GetScrapeSessionAsync(string source, ScraperType type,
        CancellationToken cancellationToken = default);

    Task CreateMovieAsync(ScrapeSession session, CreateMovieRequest request, CancellationToken cancellationToken = default);

    Task CreateLocalMovieAsync(ScrapeSession session, CreateLocalMovieRequest request,
        CancellationToken cancellationToken = default);
}

public class MovieService : IMovieService
{
    private readonly ISystemClock _clock;
    private readonly IMovieRepository _repository;
    private readonly ILogger<MovieService> _logger;
    private readonly IValidator<CreateMovieRequest> _createMovieRequestValidator;

    public MovieService(ISystemClock clock, IMovieRepository repository, ILogger<MovieService> logger, IValidator<CreateMovieRequest> createMovieRequestValidator)
    {
        _clock = clock;
        _repository = repository;
        _logger = logger;
        _createMovieRequestValidator = createMovieRequestValidator;
    }

    public async Task SetStatusAsync(string imdbCode, MovieDownloadStatusCode status,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("scraped {ImdbCode}, updating download status to {Status}", imdbCode, status);
        await _repository.AddDownloadStatus(imdbCode, GetStatus(status), cancellationToken);
    }

    public async Task<ScrapeSession> GetScrapeSessionAsync(string source, ScraperType type,
        CancellationToken cancellationToken = default)
    {
        var scrapeFrom = type == ScraperType.Local
            ? await _repository.GetLatestLocalMovieBySourceAsync(source, cancellationToken)
            : await _repository.GetLatestMovieBySourceAsync(source, cancellationToken);

        return new ScrapeSession(_clock.UtcNow, source, type, scrapeFrom);
    }

    public async Task CreateMovieAsync(ScrapeSession session, CreateMovieRequest request, CancellationToken cancellationToken = default)
    {
        if (session.Type != ScraperType.Torrent)
        {
            throw new InvalidOperationException($"scrape session of type {session.Type} cannot create torrent movies");
        }
        
        await _createMovieRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

        var imdbCode = request.ImdbCode.Trim().ToLower();
        var meta = new MovieMeta
        {
            Description = request.Description,
            Language = request.Language,
            Rating = Math.Min(Math.Max(Math.Round(request.Rating, 1), 0), 10),
            Title = request.Title,
            Year = request.Year,
            Source = session.Source,
            YouTubeTrailerCode = request.YouTubeTrailerCode,
            Genres = request.Genres.Select(x => x.Trim()).ToHashSet(),
            DateCreated = request.DateCreated,
            DateScraped = session.ScrapeDate,
            RemoteImageUrl = request.SourceCoverImageUrl
        };

        var torrents = request.Torrents
            .Select(x => new Torrent
            {
                Source = session.Source,
                Url = x.Url,
                Quality = x.Quality,
                Type = x.Type,
                Hash = x.Hash,
                SizeBytes = x.SizeBytes
            })
            .ToList();

        await _repository.UpsertFromRemoteAsync(imdbCode, meta, torrents, cancellationToken);
    }

    public async Task CreateLocalMovieAsync(ScrapeSession session, CreateLocalMovieRequest request,
        CancellationToken cancellationToken = default)
    {
        if (session.Type != ScraperType.Local)
        {
            throw new InvalidOperationException($"scrape session of type {session.Type} cannot create local movies");
        }

        var imdbCode = request.ImdbCode.Trim().ToLower();

        var meta = new MovieMeta
        {
            Source = session.Source,
            Title = request.Title,
            Year = request.Year,
            DateCreated = request.DateCreated,
            DateScraped = session.ScrapeDate,
            RemoteImageUrl = request.ThumbPath
        };

        var source = new LocalMovieSource
        {
            Source = session.Source,
            DateCreated = request.DateCreated,
            DateScraped = session.ScrapeDate
        };

        await _repository.UpsertFromLocalAsync(imdbCode, meta, source, cancellationToken);
    }

    private MovieDownloadStatus GetStatus(MovieDownloadStatusCode code) =>
        new() {Status = code, DateCreated = _clock.UtcNow};
}