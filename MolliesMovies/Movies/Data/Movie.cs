using System.Collections.Generic;
using MolliesMovies.Transmission.Data;

namespace MolliesMovies.Movies.Data
{
    public class Movie
    {
        public int Id { get; set; }
        
        public string MetaSource { get; set; }
        
        public string ImdbCode { get; set; }
        
        public string Title { get; set; }
        
        public string Language { get; set; }
        
        public int Year { get; set; }
        
        public decimal Rating { get; set; }
        
        public string Description { get; set; }
        
        public string YouTubeTrailerCode { get; set; }
        
        public string ImageFilename { get; set; }
        
        public virtual ICollection<MovieGenre> MovieGenres { get; set; }
        
        public virtual ICollection<MovieSource> MovieSources { get; set; }
        
        public virtual ICollection<DownloadedMovie> DownloadedMovies { get; set; }
        
        public virtual ICollection<TransmissionContext> TransmissionContexts { get; set; }
    }
}