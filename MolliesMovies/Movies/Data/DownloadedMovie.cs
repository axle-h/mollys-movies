namespace MolliesMovies.Movies.Data
{
    public class DownloadedMovie
    {
        public int Id { get; set; }
        
        public string MovieImdbCode { get; set; }
        
        public string LocalMovieImdbCode { get; set; }
        
        public virtual LocalMovie LocalMovie { get; set; }
    }
}