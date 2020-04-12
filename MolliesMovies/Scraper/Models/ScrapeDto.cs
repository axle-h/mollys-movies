using System;
using System.Collections.Generic;

namespace MolliesMovies.Scraper.Models
{
    public class ScrapeDto
    {
        public int Id { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public int LocalMovieCount { get; set; }
        
        public int MovieCount { get; set; }
        
        public int TorrentCount { get; set; }
        
        public bool Success { get; set; }
        
        public virtual ICollection<ScrapeSourceDto> ScrapeSources { get; set; }
    }
}