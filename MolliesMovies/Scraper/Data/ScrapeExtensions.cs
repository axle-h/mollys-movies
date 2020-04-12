using System.Linq;
using Microsoft.EntityFrameworkCore;
using MolliesMovies.Common.Data;

namespace MolliesMovies.Scraper.Data
{
    public static class ScrapeExtensions
    {
        public static IQueryable<Scrape> Scrapes(this MolliesMoviesContext context) =>
            context.Set<Scrape>().Include(x => x.ScrapeSources);
    }
}