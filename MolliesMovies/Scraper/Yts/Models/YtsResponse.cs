namespace MolliesMovies.Scraper.Yts.Models
{
    public class YtsResponse<TData>
    {
        public string Status { get; set; }
        
        public string StatusMessage { get; set; }
        
        public TData Data { get; set; }
    }
}