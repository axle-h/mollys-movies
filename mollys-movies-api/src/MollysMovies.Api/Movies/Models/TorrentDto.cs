namespace MollysMovies.Api.Movies.Models;

public record TorrentDto(
    string Source,
    string Url,
    string Hash,
    string Quality,
    string Type,
    long SizeBytes
);