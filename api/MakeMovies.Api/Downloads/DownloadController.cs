using MakeMovies.Api.Movies;
using Microsoft.AspNetCore.Mvc;

namespace MakeMovies.Api.Downloads;

[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class DownloadController(
    IMovieRepository movieRepository,
    IDownloadRepository downloadRepository,
    ITorrentService torrentService) : ControllerBase
{
    [HttpPost("movie/{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Download>> DownloadMovieAsync(string id, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetAsync(id, cancellationToken);
        if (movie is null)
        {
            return NotFound($"cannot find movie with id '{id}'");
        }

        if (movie.InLibrary)
        {
            return BadRequest($"movie with id '{id}' is already downloaded");
        }
        
        if (await downloadRepository.GetByMovieIdAsync(movie.Id, cancellationToken) is not null)
        {
            return BadRequest($"movie with id '{id}' is currently downloading");
        }

        var torrent = torrentService.SelectTorrent(movie);
        if (torrent is null)
        {
            return BadRequest($"cannot download movie with id = '{id}', no acceptable torrents available");
        }

        var transmissionId =
            await torrentService.DownloadTorrentAsync(torrent.Name, torrent.MagnetUri, cancellationToken);
        var downloadId = $"transmission_{transmissionId}";
        var download = new Download(downloadId, movie.Id, transmissionId, torrent.Name, DateTime.UtcNow, false);
        await downloadRepository.AddAsync(download, CancellationToken.None);
        return download;
    }

    [HttpGet("download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedData<Download>>> ListAsync(
        [FromQuery] PaginatedQuery<DownloadField> query, CancellationToken cancellationToken = default) =>
        await downloadRepository.ListAsync(query, cancellationToken);
}