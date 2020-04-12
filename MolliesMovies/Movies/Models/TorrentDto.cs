namespace MolliesMovies.Movies.Models
{
    public class TorrentDto
    {
        public int Id { get; set; }
        
        public string Url { get; set; }
        
        public string Hash { get; set; }
        
        public string Quality { get; set; }
        
        public string Type { get; set; }
        
        public long SizeBytes { get; set; }
    }
}