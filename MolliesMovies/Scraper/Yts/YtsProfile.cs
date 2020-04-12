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
                .ForMember(x => x.SourceUrl, o => o.MapFrom(x => x.Url))
                .ForMember(x => x.SourceId, o => o.MapFrom(x => x.Id))
                .ForMember(x => x.Description, o => o.MapFrom(x => x.DescriptionFull))
                .ForMember(x => x.DateCreated, o => o.MapFrom(x => x.DateUploaded));

            CreateMap<YtsTorrent, CreateTorrentRequest>();
        }
    }
}