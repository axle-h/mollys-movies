namespace MollysMovies.Api.Common;

public record PaginatedRequest
{
    public int? Page { get; init; }

    public int? Limit { get; init; }
}