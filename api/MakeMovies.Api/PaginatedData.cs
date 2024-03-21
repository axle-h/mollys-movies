namespace MakeMovies.Api;

public record PaginatedData<TEntity>(int Page, int Limit, int Count, IList<TEntity> Data);

public record PaginatedQuery<TField>(
    int Page = 1,
    int Limit = 10,
    TField? OrderBy = default,
    bool Descending = false,
    string? Search = null
) where TField : Enum;