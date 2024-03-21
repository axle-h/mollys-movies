using MakeMovies.Api.Downloads.TransmissionRpc;
using MakeMovies.Api.Library;
using MakeMovies.Api.Movies;
using Microsoft.Extensions.Options;

namespace MakeMovies.Api.Downloads;

public record SelectedTorrent(string Name, string Quality, string Type, string MagnetUri);

public interface ITorrentService
{
    SelectedTorrent? SelectTorrent(Movie movie);
    
    Task<int> DownloadTorrentAsync(string name, string magnetUri, CancellationToken cancellationToken = default);
}

public class TorrentService : BackgroundService, ITorrentService
{
    private readonly DownloadOptions _options;
    private readonly ILogger<TorrentService> _logger;
    private readonly IDownloadRepository _downloadRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ITransmissionRpcClient _transmissionClient;
    private readonly ILibraryService _libraryService;
    
    public TorrentService(
        IOptions<DownloadOptions> options,
        ILogger<TorrentService> logger,
        IDownloadRepository downloadRepository,
        IMovieRepository movieRepository,
        ITransmissionRpcClient transmissionClient,
        ILibraryService libraryService)
    {
        _logger = logger;
        _downloadRepository = downloadRepository;
        _transmissionClient = transmissionClient;
        _libraryService = libraryService;
        _movieRepository = movieRepository;
        _options = options.Value;
        if (_options.Trackers.Count == 0)
        {
            throw new Exception("no yts trackers configured");
        }
    }
    
    public async Task<int> DownloadTorrentAsync(string name, string magnetUri,
        CancellationToken cancellationToken = default)
    {
        var torrents = await _transmissionClient.GetAllTorrentsAsync(cancellationToken);
        if (torrents.Any(x => x.Name == name))
        {
            throw new Exception($"{name} is already downloading");
        }

        var result = await _transmissionClient.AddTorrentAsync(magnetUri, cancellationToken);
        return result.Id;
    }
    
    public SelectedTorrent? SelectTorrent(Movie movie)
    {
        var preferredTorrent = movie.Torrents
            .Select(torrent => (torrent,
                qualityIndex: _options.PreferredQuality.IndexOf(torrent.Quality),
                typeIndex: _options.PreferredType.IndexOf(torrent.Type)))
            .Where(x => x is { qualityIndex: >= 0, typeIndex: >= 0 })
            .OrderBy(x => x.qualityIndex)
            .ThenBy(x => x.typeIndex)
            .Select(x => x.torrent)
            .FirstOrDefault();

        if (preferredTorrent is null)
        {
            // TODO maybe select any?
            return null;
        }
        
        var name = $"{movie.Title} ({movie.Year})";
        var trs = string.Join('&', _options.Trackers.Select(x => $"tr={x}"));
        var magnetUri = $"magnet:?xt=urn:btih:{preferredTorrent.Hash}&dn={Uri.EscapeDataString(name)}&{trs}";
        return new SelectedTorrent(
            name,
            preferredTorrent.Quality,
            preferredTorrent.Type,
            magnetUri);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.BackgroundJobPeriod);
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            stoppingToken.ThrowIfCancellationRequested();
            try
            {
                await ProcessCompletedTorrentsAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "failed to process completed torrents");
            }
        }
    }

    private async Task ProcessCompletedTorrentsAsync()
    {
        var downloads = await _downloadRepository.AllActiveAsync();
        foreach (var download in downloads)
        {
            var downloadAge = DateTime.UtcNow - download.StartDate;
            if (downloadAge < _options.DownloadGracePeriod)
            {
                // dont bother checking brand new downloads
                continue;
            }
            
            try
            {
                var torrent = await _transmissionClient.GetTorrentByIdAsync(download.TransmissionId);
                if (torrent is null)
                {
                    // presume a missing torrent means the complete script ran and removed it ie we're free to add to library
                    await ProcessCompleteDownloadAsync(download);
                }
                else
                {
                    await UpdateTorrentInfoAsync(download, torrent);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process download {Id}", download.Id);
            }
        }
    }

    private async Task UpdateTorrentInfoAsync(Download download, TorrentInfo torrent)
    {
        var stats = new DownloadStats(
            torrent.Name,
            torrent.PercentDone,
            torrent.IsStalled,
            torrent.Eta <= 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(torrent.Eta),
            torrent.Files.Select(x => x.Name).ToHashSet());
        await _downloadRepository.UpdateAsync(download with { Stats = stats });
    }

    private async Task ProcessCompleteDownloadAsync(Download download)
    {
        _logger.LogInformation("completing active download '{Id}'", download.Id);
        _libraryService.MoveDownloadedMovieIntoLibrary(download);
        await _libraryService.UpdateLibraryAsync();
        await _downloadRepository.UpdateAsync(download with { Complete = true, Stats = null });

        var movie = await _movieRepository.GetAsync(download.MovieId)
            ?? throw new Exception($"cannot find movie {download.MovieId}");
        await _movieRepository.UpdateLibraryMoviesAsync(new HashSet<string> { movie.ImdbCode });
    }
}