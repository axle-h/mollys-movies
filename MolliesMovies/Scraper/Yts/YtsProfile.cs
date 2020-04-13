using System;
using AutoMapper;
using MolliesMovies.Movies.Models;
using MolliesMovies.Movies.Requests;
using MolliesMovies.Scraper.Yts.Models;

namespace MolliesMovies.Scraper.Yts
{
    public class YtsProfile : Profile
    {
        public YtsProfile()
        {
            CreateMap<YtsMovieSummary, CreateMovieRequest>()
                .ForMember(x => x.SourceUrl, o => o.MapFrom(x => GetPathAndQuery(x.Url)))
                .ForMember(x => x.SourceId, o => o.MapFrom(x => x.Id))
                .ForMember(x => x.Description, o => o.MapFrom(x => string.IsNullOrEmpty(x.DescriptionFull) ? null : x.DescriptionFull))
                .ForMember(x => x.SourceCoverImageUrl, o => o.MapFrom(x => GetPathAndQuery(x.LargeCoverImage)))
                .ForMember(x => x.YouTubeTrailerCode, o => o.MapFrom(x => string.IsNullOrEmpty(x.YtTrailerCode) ? null : x.YtTrailerCode))
                .ForMember(x => x.DateCreated, o => o.MapFrom(x => x.DateUploaded));

            CreateMap<YtsTorrent, CreateTorrentRequest>();
        }

        private static string GetPathAndQuery(string url)
        {
            if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                return null;
            }

            return uri.PathAndQuery;
        }
    }
}