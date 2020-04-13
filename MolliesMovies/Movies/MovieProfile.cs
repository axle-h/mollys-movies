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
            CreateMap<Movie, MovieImageSourcesDto>()
                .ForMember(x => x.LocalSource, o => o.MapFrom(x => x.DownloadedMovies.Select(y => y.LocalMovie).FirstOrDefault(y => !string.IsNullOrEmpty(y.ThumbPath))))
                .ForMember(x => x.RemoteSources, o => o.MapFrom(x => x.MovieSources.Where(y => !string.IsNullOrEmpty(y.SourceCoverImageUrl))));

            CreateMap<LocalMovie, MovieImageSourceDto>()
                .ForMember(x => x.Value, o => o.MapFrom(x => x.ThumbPath));

            CreateMap<MovieSource, MovieImageSourceDto>()
                .ForMember(x => x.Value, o => o.MapFrom(x => x.SourceCoverImageUrl));
        }
    }
}