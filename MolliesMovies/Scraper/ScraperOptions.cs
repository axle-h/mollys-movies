using System;
using System.Collections.Generic;

namespace MolliesMovies.Scraper
{
    public class ScraperOptions
    {
        public TimeSpan RemoteScrapeDelay { get; set; }
        
        public PlexOptions Plex { get; set; }
    }
    
    public class PlexOptions
    {
        public string Token { get; set; }
    }
}