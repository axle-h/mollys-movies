using System;
using MolliesMovies.Scraper.Data;

namespace MolliesMovies.Scraper.Models
{
    public class ScrapeSourceDto
    {
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