using System;

namespace MolliesMovies.Movies.Data
{
    public class LocalMovie
    {
        public int Id { get; set; }
        
        public string Source { get; set; }
        
        public string ImdbCode { get; set; }
        
        public string Title { get; set; }
        
        public int Year { get; set; }
        
        public string ThumbPath { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        public DateTime DateScraped { get; set; }
    }
}