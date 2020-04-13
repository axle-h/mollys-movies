using System;
using System.Collections.Generic;

namespace MolliesMovies.Movies.Requests
{
    public class CreateMovieRequest
    {
        public string ImdbCode { get; set; }
        
        public string Title { get; set; }
        
        public string Language { get; set; }
        
        public int Year { get; set; }
        
        public decimal Rating { get; set; }
        
        public string Description { get; set; }

        public ICollection<string> Genres { get; set; }
        
        public string YouTubeTrailerCode { get; set; }
        
        public string SourceCoverImageUrl { get; set; }
        
        public string SourceUrl { get; set; }
        
        public string SourceId { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        public ICollection<CreateTorrentRequest> Torrents { get; set; }
    }
}