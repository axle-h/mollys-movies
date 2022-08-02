using System.Threading.Tasks;
using MassTransit;
using MollysMovies.Scraper.Movies;
using MollysMovies.ScraperClient;

namespace MollysMovies.Scraper.Consumer;

public class NotifyDownloadCompleteConsumer : IConsumer<NotifyDownloadComplete>
{
    private readonly IMovieDownloadCompleteService _service;

    public NotifyDownloadCompleteConsumer(IMovieDownloadCompleteService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<NotifyDownloadComplete> context)
    {
        var notification = await _service.CompleteActiveAsync(context.Message.ExternalId, context.CancellationToken);
        await context.Publish(notification);
    }
}