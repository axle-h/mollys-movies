namespace MolliesMovies.Movies.Requests
{
    public class SearchMoviesRequest
    {
        public int? Page { get; set; }
        
        public int? Limit { get; set; }
        
        public string Title { get; set; }
        
        public string Quality { get; set; }
        
        public string Language { get; set; }

        public MoviesOrderBy? OrderBy { get; set; }
        
        public bool? OrderByDescending { get; set; }
        
        public bool? Downloaded { get; set; }
    }
}