using System.Collections.Generic;
using MolliesMovies.Transmission.Data;

namespace MolliesMovies.Movies.Models
{
    public class MovieDto
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

        public ICollection<string> Genres { get; set; }
        
        public ICollection<MovieSourceDto> MovieSources { get; set; }
        
        public LocalMovieDto LocalMovie { get; set; }
        
        public TransmissionStatusCode? TransmissionStatus { get; set; }
    }
}