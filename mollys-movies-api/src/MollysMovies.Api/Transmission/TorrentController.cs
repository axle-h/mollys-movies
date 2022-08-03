using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MollysMovies.Api.Common.Routing;
using MollysMovies.Api.Transmission.Models;

namespace MollysMovies.Api.Transmission;

[PublicApiRoute("/movies/{imdbCode}/torrents")]
public class TorrentController : ControllerBase
{
    private readonly ITorrentService _service;

    public TorrentController(ITorrentService service)
    {
        _service = service;
    }

    [HttpPost("quality/{quality}/type/{type}")]
    public async Task DownloadMovie(string imdbCode, string quality, string type, CancellationToken cancellationToken = default) =>
        await _service.DownloadMovieTorrentAsync(imdbCode, quality, type, cancellationToken);

    [HttpGet]
    public async Task<LiveTransmissionStatusDto> GetLiveTransmissionStatus(string imdbCode,
        CancellationToken cancellationToken = default) =>
        await _service.GetLiveTransmissionStatusAsync(imdbCode, cancellationToken);
}