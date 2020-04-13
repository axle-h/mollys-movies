namespace MolliesMovies.Movies.Requests
{
    public class CreateMovieImageRequest
    {
        public string ImdbCode { get; set; }
        
        public byte[] Content { get; set; }
        
        public string ContentType { get; set; }
    }
}