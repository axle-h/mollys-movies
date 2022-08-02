using System;
using System.Threading.Tasks;
using MassTransit;
using MollysMovies.Common.Scraper;
using MollysMovies.ScraperClient;

namespace MollysMovies.Scraper.Consumer;

public class StartScrapeConsumer : IConsumer<StartScrape>
{
    private readonly IScraperService _service;

    public StartScrapeConsumer(IScraperService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<StartScrape> context)
    {
        var notification = await _service.ScrapeAsync(context.Message.Id, context.CancellationToken);
        await context.Publish(notification);
    }
}