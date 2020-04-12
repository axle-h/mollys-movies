using System.Collections.Generic;

namespace MolliesMovies.Common
{
    public class Paginated<TDto>
    {
        public int Page { get; set; }
        
        public int Limit { get; set; }
        
        public int Count { get; set; }
        
        public ICollection<TDto> Data { get; set; }
    }
}