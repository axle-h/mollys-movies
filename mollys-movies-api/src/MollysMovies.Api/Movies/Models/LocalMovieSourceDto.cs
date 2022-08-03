using System;

namespace MollysMovies.Api.Movies.Models;

public record LocalMovieSourceDto(
    string Source,
    DateTime DateCreated,
    DateTime DateScraped
);