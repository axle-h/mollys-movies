namespace MollysMovies.Api.Transmission.Models;

/// <summary>
/// </summary>
/// <param name="Name">The full name of the movie.</param>
/// <param name="Complete">Whether the download is complete.</param>
/// <param name="Stalled">Whether the download is stalled due to lack of seeds.</param>
/// <param name="Eta">The total estimated seconds until this download is done.</param>
/// <param name="PercentComplete">The percentage complete 0...1.</param>
public record LiveTransmissionStatusDto(
    string Name,
    bool Complete,
    bool? Stalled = null,
    int? Eta = null,
    double? PercentComplete = null)
{
    public static LiveTransmissionStatusDto GetComplete(string name) => new(name, true);
}