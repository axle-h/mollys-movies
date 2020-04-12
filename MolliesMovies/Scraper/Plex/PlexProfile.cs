using AutoMapper;
using MolliesMovies.Movies.Requests;
using MolliesMovies.Scraper.Plex.Models;

namespace MolliesMovies.Scraper.Plex
{
    public class PlexProfile : Profile
    {
        public PlexProfile()
        {
            CreateMap<PlexMovie, CreateLocalMovieRequest>();
        }
    }
}