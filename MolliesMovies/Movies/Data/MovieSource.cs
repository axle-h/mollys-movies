using System;
using System.Collections.Generic;

namespace MolliesMovies.Movies.Data
{
    public class MovieSource
    {
        public int Id { get; set; }
        
        public int MovieId { get; set; }
        
        public string Source { get; set; }
        
        public string SourceUrl { get; set; }
        
        public string SourceId { get; set; }
        
        public string SourceCoverImageUrl { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        public DateTime DateScraped { get; set; }
        
        public virtual ICollection<Torrent> Torrents { get; set; }
    }
}