using System;
using System.Collections.Generic;

namespace MolliesMovies.Movies.Models
{
    public class MovieSourceDto
    {
        public string Source { get; set; }
        
        public string SourceUrl { get; set; }
        
        public string SourceId { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        public DateTime DateScraped { get; set; }
        
        public ICollection<TorrentDto> Torrents { get; set; }
    }
}