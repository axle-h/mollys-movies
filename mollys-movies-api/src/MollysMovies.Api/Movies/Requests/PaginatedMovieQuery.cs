using MollysMovies.Api.Common;
using MollysMovies.Common.Movies;

namespace MollysMovies.Api.Movies.Requests;

public record PaginatedMovieQuery : PaginatedQuery<Movie>
{
    public string? Text { get; init; }

    public string? Quality { get; init; }

    public string? Language { get; init; }

    public bool? HasDownload { get; init; }

    public bool? Downloaded { get; init; }

    public string? Genre { get; init; }

    public int? YearFrom { get; init; }

    public int? YearTo { get; init; }

    public decimal? RatingFrom { get; init; }

    public decimal? RatingTo { get; init; }
}