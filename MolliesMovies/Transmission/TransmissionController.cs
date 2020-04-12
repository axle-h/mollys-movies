using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MolliesMovies.Transmission.Models;

namespace MolliesMovies.Transmission
{
    [Route("/api/movies/{movieId:int}/torrents")]
    public class TransmissionController : ControllerBase
    {
        private readonly ITransmissionService _service;

        public TransmissionController(ITransmissionService service)
        {
            _service = service;
        }

        [HttpPost("{torrentId:int}/download")]
        public async Task Download(int movieId, int torrentId, CancellationToken cancellationToken = default) =>
            await _service.DownloadMovieTorrentAsync(movieId, torrentId, cancellationToken);

        [HttpGet("{torrentId:int}/live-status")]
        public async Task<LiveTransmissionStatusDto> GetLiveTransmissionStatusAsync(int movieId, int torrentId,
            CancellationToken cancellationToken = default) =>
            await _service.GetLiveTransmissionStatusAsync(movieId, torrentId, cancellationToken);
    }
}