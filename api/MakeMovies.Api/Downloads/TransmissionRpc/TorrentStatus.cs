namespace MakeMovies.Api.Downloads.TransmissionRpc;

/// <summary>
/// </summary>
/// <param name="TransmissionId">Transmission id.</param>
/// <param name="Complete">Whether the download is complete.</param>
/// <param name="Stalled">Whether the download is stalled due to lack of seeds.</param>
/// <param name="Eta">The total estimated time until this download is done.</param>
/// <param name="PercentComplete">The percentage complete 0...1.</param>
public record TorrentStatus(
    int TransmissionId,
    bool Complete,
    bool? Stalled = null,
    TimeSpan? Eta = null,
    double? PercentComplete = null);