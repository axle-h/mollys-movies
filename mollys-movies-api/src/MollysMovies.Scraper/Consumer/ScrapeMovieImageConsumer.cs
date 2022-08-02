using System.Threading.Tasks;
using MassTransit;
using MollysMovies.Common.Scraper;
using MollysMovies.Scraper.Image;
using MollysMovies.ScraperClient;

namespace MollysMovies.Scraper.Consumer;

public class ScrapeMovieImageConsumer : IConsumer<ScrapeMovieImage>
{
    private readonly IImageScraperService _service;

    public ScrapeMovieImageConsumer(IImageScraperService service)
    {
        _service = service;
    }

    public Task Consume(ConsumeContext<ScrapeMovieImage> context) =>
        _service.ScrapeImageAsync(context.Message, context.CancellationToken);
}