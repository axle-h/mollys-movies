using System;

namespace MolliesMovies.Scraper.Data
{
    public class ScrapeSource
    {
        public int Id { get; set; }
        
        public int ScrapeId { get; set; }
        
        public string Source { get; set; }
        
        public ScraperType Type { get; set; }
        
        public bool Success { get; set; }
        
        public string Error { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public int MovieCount { get; set; }
        
        public int TorrentCount { get; set; }
    }
}