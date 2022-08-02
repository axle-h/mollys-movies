using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MollysMovies.Api.Common;

public class PaginatedData<TEntity>
{
    public int Page { get; init; }
    public int Limit { get; init; }
    public long Count { get; init; }
    public ICollection<TEntity> Data { get; init; } = new List<TEntity>();
}

public abstract record PaginatedQuery<TEntity>
{
    public int Page { get; init; }

    public int Limit { get; init; }

    public ICollection<PaginatedOrderBy<TEntity>>? OrderBy { get; init; }
}

public record PaginatedOrderBy<TEntity>(Expression<Func<TEntity, object?>> Property, bool Descending);