using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MolliesMovies.Common.Routing;
using MolliesMovies.Transmission.Models;

namespace MolliesMovies.Transmission
{
    [PublicApiRoute("/movies/{movieId:int}/torrents")]
    public class MovieTorrentController : ControllerBase
    {
        private readonly ITransmissionService _service;

        public MovieTorrentController(ITransmissionService service)
        {
            _service = service;
        }

        [HttpPost("{torrentId:int}/download")]
        public async Task DownloadMovie(int movieId, int torrentId, CancellationToken cancellationToken = default) =>
            await _service.DownloadMovieTorrentAsync(movieId, torrentId, cancellationToken);

        [HttpGet("{torrentId:int}/live-status")]
        public async Task<LiveTransmissionStatusDto> GetLiveTransmissionStatus(int movieId, int torrentId,
            CancellationToken cancellationToken = default) =>
            await _service.GetLiveTransmissionStatusAsync(movieId, torrentId, cancellationToken);
    }
}