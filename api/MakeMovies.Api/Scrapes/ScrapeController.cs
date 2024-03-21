using Microsoft.AspNetCore.Mvc;

namespace MakeMovies.Api.Scrapes;

[ApiController]
[Route("api/v1/scrape")]
[Produces("application/json")]
public class ScrapeController(IScrapeService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<PaginatedData<Scrape>> ListAsync([FromQuery] PaginatedQuery<ScrapeField> query,
        CancellationToken cancellationToken = default) =>
        await service.ListAsync(query, cancellationToken);
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Scrape>> StartScrapeAsync(CancellationToken cancellationToken = default)
    {
        var scrape = await service.StartScrapeAsync(cancellationToken);
        return scrape is null ? BadRequest("the scraper is already running") : Ok(scrape);
    }
}