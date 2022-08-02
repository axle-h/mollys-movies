using MollysMovies.Common.Movies;

namespace MollysMovies.Api.Movies.Models;

public record MovieDownloadDto(
    string ImdbCode,
    string ExternalId,
    string Name,
    MovieDownloadStatusCode Status);