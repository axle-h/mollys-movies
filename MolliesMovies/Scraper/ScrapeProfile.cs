using AutoMapper;
using MolliesMovies.Scraper.Data;
using MolliesMovies.Scraper.Models;

namespace MolliesMovies.Scraper
{
    public class ScrapeProfile : Profile
    {
        public ScrapeProfile()
        {
            CreateMap<Scrape, ScrapeDto>();
            CreateMap<ScrapeSource, ScrapeSourceDto>();
        }
    }
}