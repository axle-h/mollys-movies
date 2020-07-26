using MolliesMovies.Common;

namespace MolliesMovies.Movies.Requests
{
    public class SearchMoviesRequest : PaginatedRequest
    {
        public string Title { get; set; }
        
        public string Quality { get; set; }
        
        public string Language { get; set; } 

        public bool? Downloaded { get; set; }
        
        public string Genre { get; set; }
        
        public int? YearFrom { get; set; }
        
        public int? YearTo { get; set; }
        
        public int? RatingFrom { get; set; }
        
        public int? RatingTo { get; set; }
        
        public MoviesOrderBy? OrderBy { get; set; }
        
        public bool? OrderByDescending { get; set; }
    }
}