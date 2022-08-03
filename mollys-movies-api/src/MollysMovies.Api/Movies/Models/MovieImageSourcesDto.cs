using System.Collections.Generic;

namespace MollysMovies.Api.Movies.Models;

public record MovieImageSourcesDto(
    int Id,
    string ImdbCode,
    MovieImageSourceDto? LocalSource,
    ICollection<MovieImageSourceDto> RemoteSources
);