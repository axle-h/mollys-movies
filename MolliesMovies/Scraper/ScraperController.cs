using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MolliesMovies.Common.Routing;
using MolliesMovies.Scraper.Models;

namespace MolliesMovies.Scraper
{
    [PublicApiRoute]
    public class ScraperController : ControllerBase
    {
        private readonly IScraperService _service;
        private readonly IScraperBackgroundService _backgroundService;

        public ScraperController(IScraperService service, IScraperBackgroundService backgroundService)
        {
            _service = service;
            _backgroundService = backgroundService;
        }

        [HttpGet]
        public async Task<ICollection<ScrapeDto>> GetAllScrapes(CancellationToken cancellationToken = default) =>
            await _service.GetAllAsync(cancellationToken);

        [HttpPost]
        public async Task<ScrapeDto> Scrape(CancellationToken cancellationToken = default)
        {
            var scrape = await _service.CreateScrapeAsync(cancellationToken);
            _backgroundService.AddScrapeAllJob(scrape.Id);
            return scrape;
        }
    }
}