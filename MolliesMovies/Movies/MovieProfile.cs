using System.Linq;
using AutoMapper;
using MolliesMovies.Movies.Data;
using MolliesMovies.Movies.Models;
using MolliesMovies.Transmission.Data;

namespace MolliesMovies.Movies
{
    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            CreateMap<Movie, MovieDto>()
                .ForMember(x => x.Genres, o => o.MapFrom(x => x.MovieGenres.Select(g => g.Genre.Name)))
                .ForMember(x => x.LocalMovie, o => o.MapFrom(x => x.DownloadedMovies
                    .Select(dm => dm.LocalMovie)
                    .FirstOrDefault()))
                .ForMember(x => x.TransmissionStatus, o => o.MapFrom(x => x.TransmissionContexts
                    .SelectMany(c => c.Statuses)
                    .OrderByDescending(s => s.DateCreated)
                    .Select<TransmissionContextStatus, TransmissionStatusCode?>(s => s.Status)
                    .FirstOrDefault()));
            CreateMap<MovieSource, MovieSourceDto>();
            CreateMap<Torrent, TorrentDto>();
            CreateMap<LocalMovie, LocalMovieDto>();
        }
    }
}