using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MollysMovies.Api.Common.Routing;
using MollysMovies.Api.Scraper.Models;
using MollysMovies.Common.Scraper;
using MollysMovies.ScraperClient;

namespace MollysMovies.Api.Scraper;

[PublicApiRoute]
public class ScrapeController : ControllerBase
{
    private readonly IScraperClient _scraperClient;
    private readonly IScrapeService _service;

    public ScrapeController(IScrapeService service, IScraperClient scraperClient)
    {
        _service = service;
        _scraperClient = scraperClient;
    }

    [HttpGet]
    public async Task<ICollection<ScrapeDto>> GetAllScrapes(CancellationToken cancellationToken = default) =>
        await _service.GetAllAsync(cancellationToken);

    [HttpPost]
    public async Task<ScrapeDto> Scrape(CancellationToken cancellationToken = default)
    {
        var scrape = await _service.CreateScrapeAsync(cancellationToken);
        await _scraperClient.StartScrapeAsync(scrape.Id, cancellationToken);
        return scrape;
    }
}