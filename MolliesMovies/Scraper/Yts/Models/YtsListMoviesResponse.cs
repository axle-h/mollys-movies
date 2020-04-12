using System.Collections.Generic;

namespace MolliesMovies.Scraper.Yts.Models
{
    public class YtsListMoviesResponse
    {
        public int MovieCount { get; set; }
        
        public int Limit { get; set; }
        
        public int PageNumber { get; set; }
        
        public ICollection<YtsMovieSummary> Movies { get; set; }
    }
}