using System.Collections.Generic;

namespace MolliesMovies.Movies.Models
{
    public class MovieImageSourcesDto
    {
        public int Id { get; set; }
        
        public string ImdbCode { get; set; }
        
        public MovieImageSourceDto LocalSource { get; set; }
        
        public ICollection<MovieImageSourceDto> RemoteSources { get; set; }
    }
}