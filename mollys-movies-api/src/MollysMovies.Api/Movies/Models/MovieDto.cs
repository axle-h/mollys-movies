using System.Collections.Generic;

namespace MollysMovies.Api.Movies.Models;

public record MovieDto(
    string ImdbCode,
    string Title,
    string? Language,
    int Year,
    decimal? Rating,
    string? Description,
    string? YouTubeTrailerCode,
    string? ImageFilename,
    ICollection<string> Genres,
    ICollection<TorrentDto> Torrents,
    LocalMovieSourceDto? LocalSource,
    MovieDownloadDto? Download
);