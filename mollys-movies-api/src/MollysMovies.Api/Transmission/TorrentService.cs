using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MollysMovies.Api.Common.Exceptions;
using MollysMovies.Api.Movies;
using MollysMovies.Api.Movies.Requests;
using MollysMovies.Api.Transmission.Models;
using MollysMovies.Common.Movies;

namespace MollysMovies.Api.Transmission;

public interface ITorrentService
{
    Task DownloadMovieTorrentAsync(string imdbCode, string quality, string type, CancellationToken cancellationToken = default);

    Task<LiveTransmissionStatusDto> GetLiveTransmissionStatusAsync(string imdbCode,
        CancellationToken cancellationToken = default);
}

public class TorrentService : ITorrentService
{
    private readonly ILogger<TorrentService> _logger;
    private readonly IMagnetUriService _magnetUriService;
    private readonly IMovieDownloadService _movieDownloadService;
    private readonly IMovieService _movieService;
    private readonly ITransmissionService _transmissionService;

    public TorrentService(
        IMovieService movieService,
        ILogger<TorrentService> logger,
        ITransmissionService transmissionService,
        IMovieDownloadService movieDownloadService,
        IMagnetUriService magnetUriService)
    {
        _movieService = movieService;
        _logger = logger;
        _transmissionService = transmissionService;
        _movieDownloadService = movieDownloadService;
        _magnetUriService = magnetUriService;
    }

    public async Task DownloadMovieTorrentAsync(string imdbCode, string quality, string type,
        CancellationToken cancellationToken = default)
    {
        var movie = await _movieService.GetAsync(imdbCode, cancellationToken);
        var torrent = movie.Torrents.FirstOrDefault(x => x.Quality == quality && x.Type == type);
        if (torrent is null)
        {
            throw BadRequestException.Create(
                $"torrent with quality '{quality}' and type '{type}' does not exist on movie with imdb code '{imdbCode}'");
        }

        if (movie.LocalSource is not null)
        {
            throw BadRequestException.Create($"movie with imdb code '{imdbCode}' is already downloaded");
        }

        if (movie.Download is not null)
        {
            throw BadRequestException.Create($"movie with imdb code '{imdbCode}' is already downloading");
        }

        var name = $"{movie.Title} ({movie.Year})";
        var magnet = _magnetUriService.BuildMagnetUri(name, torrent.Hash);
        var externalId = await _transmissionService
            .DownloadTorrentAsync(new DownloadMovieRequest(name, magnet), cancellationToken);

        _logger.LogInformation("added torrent {Name}", name);

        var request = new SetDownloadRequest(imdbCode, torrent.Source, torrent.Quality, torrent.Type, externalId, name, magnet);
        await _movieDownloadService.SetDownloadAsync(request, cancellationToken);
        _logger.LogInformation("saved context for torrent {Name}", name);
    }

    public async Task<LiveTransmissionStatusDto> GetLiveTransmissionStatusAsync(string imdbCode,
        CancellationToken cancellationToken = default)
    {
        var movie = await _movieService.GetAsync(imdbCode, cancellationToken);
        var download = movie.Download;
        if (download is null)
        {
            throw EntityNotFoundException.Of<MovieDownload>(new {ImdbCode = imdbCode});
        }

        if (download.Status != MovieDownloadStatusCode.Started)
        {
            return LiveTransmissionStatusDto.GetComplete(download.Name);
        }

        var request = new GetLiveTransmissionStatusRequest(download.ExternalId, download.Name);
        return await _transmissionService.GetLiveTransmissionStatusAsync(request, cancellationToken);
    }
}