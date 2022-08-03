namespace MollysMovies.Api.Movies.Requests;

public record SetDownloadRequest(
    string ImdbCode,
    string Source,
    string Quality,
    string Type,
    string ExternalId,
    string Name,
    string MagnetUri);