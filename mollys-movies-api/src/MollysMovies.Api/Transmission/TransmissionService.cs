using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MollysMovies.Api.Common.Exceptions;
using MollysMovies.Api.Transmission.Models;
using MollysMovies.Common.TransmissionRpc;

namespace MollysMovies.Api.Transmission;

public interface ITransmissionService
{
    Task<string> DownloadTorrentAsync(DownloadMovieRequest request, CancellationToken cancellationToken = default);

    Task<LiveTransmissionStatusDto> GetLiveTransmissionStatusAsync(
        GetLiveTransmissionStatusRequest request, CancellationToken cancellationToken = default);
}

public class TransmissionService : ITransmissionService
{
    private readonly ITransmissionRpcClient _client;

    public TransmissionService(ITransmissionRpcClient client)
    {
        _client = client;
    }

    public async Task<TorrentInfo> GetTorrentAsync(string externalId, CancellationToken cancellationToken = default)
    {
        var info = await _client.GetTorrentByIdAsync(GetTransmissionId(externalId), cancellationToken);
        return info ?? throw EntityNotFoundException.Of<TorrentInfo>(externalId);
    }

    public async Task<string> DownloadTorrentAsync(DownloadMovieRequest request,
        CancellationToken cancellationToken = default)
    {
        var torrents = await _client.GetAllTorrentsAsync(cancellationToken);
        if (torrents.Any(x => x.Name == request.Name))
        {
            throw BadRequestException.Create($"{request.Name} is already downloading");
        }

        var result = await _client.AddTorrentAsync(request.MagnetUri, cancellationToken);
        return result.Id.ToString();
    }

    public async Task<LiveTransmissionStatusDto> GetLiveTransmissionStatusAsync(
        GetLiveTransmissionStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(request.ExternalId, out var transmissionId))
        {
            throw new Exception($"movie download '{request.Name}' does not have valid transmission id");
        }

        // Get live status.
        var torrent = await _client.GetTorrentByIdAsync(transmissionId, cancellationToken);
        if (torrent is null)
        {
            // HACK: Assume if the torrent is missing then it is complete.
            return LiveTransmissionStatusDto.GetComplete(request.Name);
        }

        return new LiveTransmissionStatusDto(
            request.Name,
            !(torrent.PercentDone < 1.0),
            torrent.IsStalled,
            torrent.Eta <= 0 ? null : torrent.Eta,
            torrent.PercentDone);
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