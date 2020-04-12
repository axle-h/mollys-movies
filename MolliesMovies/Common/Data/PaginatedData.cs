using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MolliesMovies.Common.Data
{
    public class PaginatedData<TEntity>
    {
        public int Page { get; set; }
        
        public int Limit { get; set; }
        
        public int Count { get; set; }
        
        public ICollection<TEntity> Data { get; set; }
    }

    public class PaginatedQuery<TEntity>
    {
        public int Page { get; set; }
        
        public int Limit { get; set; }
        
        public ICollection<PaginatedOrderBy<TEntity>> OrderBy { get; set; }
    }

    public class PaginatedOrderBy<TEntity>
    {
        public Expression<Func<TEntity, object>> Property { get; set; }
        
        public bool Descending { get; set; }
    }
}