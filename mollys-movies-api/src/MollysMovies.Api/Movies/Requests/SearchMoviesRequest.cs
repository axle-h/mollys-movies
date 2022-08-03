using MollysMovies.Api.Common;

namespace MollysMovies.Api.Movies.Requests;

public record SearchMoviesRequest : PaginatedRequest
{
    public string? Title { get; init; }

    public string? Quality { get; init; }

    public string? Language { get; init; }

    public bool? HasDownload { get; init; }

    public bool? Downloaded { get; init; }

    public string? Genre { get; init; }

    public int? YearFrom { get; init; }

    public int? YearTo { get; init; }

    public int? RatingFrom { get; init; }

    public int? RatingTo { get; init; }

    public MoviesOrderBy? OrderBy { get; init; }

    public bool? OrderByDescending { get; init; }
}