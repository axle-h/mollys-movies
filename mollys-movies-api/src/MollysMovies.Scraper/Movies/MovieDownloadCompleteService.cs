using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MollysMovies.Common.Movies;
using MollysMovies.Common.TransmissionRpc;
using MollysMovies.ScraperClient;

namespace MollysMovies.Scraper.Movies;

public interface IMovieDownloadCompleteService
{
    Task<NotifyMovieAddedToLibrary> CompleteActiveAsync(string externalId, CancellationToken cancellationToken = default);
}

public class MovieDownloadCompleteService : IMovieDownloadCompleteService
{
    private readonly IMovieRepository _movieRepository;
    private readonly ITransmissionRpcClient _transmission;
    private readonly ILogger<MovieDownloadCompleteService> _logger;
    private readonly IMovieLibraryService _movieLibrary;
    private readonly IMovieService _movieService;
    private readonly IScraperService _scraperService;
    private readonly ScraperOptions _options;
    
    public MovieDownloadCompleteService(
        IMovieRepository movieRepository,
        ITransmissionRpcClient transmission,
        ILogger<MovieDownloadCompleteService> logger,
        IMovieLibraryService movieLibrary,
        IMovieService movieService,
        IScraperService scraperService,
        IOptions<ScraperOptions> options)
    {
        _movieRepository = movieRepository;
        _transmission = transmission;
        _logger = logger;
        _movieLibrary = movieLibrary;
        _movieService = movieService;
        _scraperService = scraperService;
        _options = options.Value;
    }

    public async Task<NotifyMovieAddedToLibrary> CompleteActiveAsync(string externalId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("completing active torrent '{ExternalId}'", externalId);
        try
        {
            var movie = await _movieRepository.GetMovieByDownloadExternalIdAsync(externalId, cancellationToken)
                        ?? throw new Exception($"movie with download external id '{externalId}' not found");

            var status = movie.Download!.Statuses.MaxBy(x => x.DateCreated)?.Status;
            if (status is null or not MovieDownloadStatusCode.Started)
            {
                throw new Exception($"movie with download external id '{externalId}' has inactive download '{status}'");
            }

            var torrent = await _transmission.GetTorrentByIdAsync(GetTransmissionId(externalId), cancellationToken)
                          ?? throw new Exception($"torrent with id '{externalId}' not found");

            _movieLibrary.AddMovie(movie.Download.Name!, torrent);

            await _movieService.SetStatusAsync(movie.ImdbCode, MovieDownloadStatusCode.Downloaded, cancellationToken);


            await _scraperService.UpdateLocalMovieLibrariesAsync(cancellationToken);

            // HACK: the update library call isn't guaranteed to block so we need to wait a bit.
            await Task.Delay(_options.LocalUpdateMovieDelay, cancellationToken);

            if (await _scraperService.ScrapeForLocalMovieAsync(movie.ImdbCode, cancellationToken))
            {
                await _movieService.SetStatusAsync(movie.ImdbCode, MovieDownloadStatusCode.Complete, cancellationToken);
                _logger.LogInformation("movie download complete callback successful '{ExternalId}'", externalId);
            }
            else
            {
                _logger.LogError("scraping for local movie failed: {ImdbCode}", movie.ImdbCode);
            }
            return new NotifyMovieAddedToLibrary(movie.ImdbCode);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "movie download complete callback failed '{ExternalId}'", externalId);
            throw;
        }
        finally
        {
            // always remove from transmission
            await _transmission.RemoveTorrentAsync(GetTransmissionId(externalId), cancellationToken);
        }
    }
    
    private static int GetTransmissionId(string externalId)
    {
        if (!int.TryParse(externalId, out var transmissionId))
        {
            throw new Exception($"movie download '{externalId}' does not have valid transmission id");
        }
        return transmissionId;
    }
}