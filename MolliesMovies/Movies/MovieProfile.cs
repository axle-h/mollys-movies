using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using MolliesMovies.Common.Data;
using MolliesMovies.Movies.Data;
using MolliesMovies.Movies.Models;
using MolliesMovies.Movies.Requests;
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
                .ForMember(x => x.LocalSource,
                    o => o.MapFrom(x =>
                        x.DownloadedMovies.Select(y => y.LocalMovie)
                            .FirstOrDefault(y => !string.IsNullOrEmpty(y.ThumbPath))))
                .ForMember(x => x.RemoteSources,
                    o => o.MapFrom(x => x.MovieSources.Where(y => !string.IsNullOrEmpty(y.SourceCoverImageUrl))));

            CreateMap<LocalMovie, MovieImageSourceDto>()
                .ForMember(x => x.Value, o => o.MapFrom(x => x.ThumbPath));

            CreateMap<MovieSource, MovieImageSourceDto>()
                .ForMember(x => x.Value, o => o.MapFrom(x => x.SourceCoverImageUrl));

            CreateMap<SearchMoviesRequest, PaginatedMovieQuery>()
                .ForMember(x => x.OrderBy, o => o.MapFrom(x => GetOrderBy(x.OrderBy, x.OrderByDescending)));
        }

        private static ICollection<PaginatedOrderBy<Movie>> GetOrderBy(MoviesOrderBy? orderBy, bool? descending)
        {
            PaginatedOrderBy<Movie> OrderBy(Expression<Func<Movie, object>> property) =>
                new PaginatedOrderBy<Movie> {Property = property, Descending = descending ?? false};

            return (orderBy ?? MoviesOrderBy.Title) switch
            {
                MoviesOrderBy.Title => new[] {OrderBy(x => x.Title)},
                MoviesOrderBy.Year => new[] {OrderBy(x => x.Year), OrderBy(x => x.Title)},
                MoviesOrderBy.Rating => new[] {OrderBy(x => x.Rating)},
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}