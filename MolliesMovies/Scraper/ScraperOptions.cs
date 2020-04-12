using System;
using System.Collections.Generic;

namespace MolliesMovies.Scraper
{
    public class ScraperOptions
    {
        public YtsOptions Yts { get; set; }
        
        public PlexOptions Plex { get; set; }
    }

    public class YtsOptions
    {
        public TimeSpan ScrapeDelay { get; set; }
    }
    
    public class PlexOptions
    {
        public string Token { get; set; }
    }
}