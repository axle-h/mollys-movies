using System;

namespace MolliesMovies.Scraper.Plex.Models
{
    public class PlexMovie
    {
        public string ImdbCode { get; set; }
        
        public string Title { get; set; }
        
        public int Year { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        public string ThumbPath { get; set; }
    }
}