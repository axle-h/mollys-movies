using System;
using System.Collections.Generic;

namespace MolliesMovies.Movies.Models
{
    public class MovieSourceDto
    {
        public string Source { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        public DateTime DateScraped { get; set; }
        
        public ICollection<TorrentDto> Torrents { get; set; }
    }
}