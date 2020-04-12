namespace MolliesMovies.Scraper.Yts.Models
{
    public class YtsTorrent
    {
        public string Url { get; set; }
        
        public string Hash { get; set; }
        
        public string Quality { get; set; }
        
        public string Type { get; set; }
        
        public string Size { get; set; }
        
        public long SizeBytes { get; set; }
        
        public string DateUploaded { get; set; }
        
        public long DateUploadedUnix { get; set; }
    }
}