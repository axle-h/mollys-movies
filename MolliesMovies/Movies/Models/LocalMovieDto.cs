using System;

namespace MolliesMovies.Movies.Models
{
    public class LocalMovieDto
    {
        public string Source { get; set; }
        
        public string Title { get; set; }
        
        public int Year { get; set; }
        
        public DateTime DateCreated { get; set; }
    }
}