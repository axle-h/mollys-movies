using MassTransit;
using MassTransit.Testing;

namespace MollysMovies.ScraperClient;

public interface IScraperClient
{
    Task NotifyDownloadCompleteAsync(string externalId, CancellationToken cancellationToken = default);

    Task StartScrapeAsync(string scrapeId, CancellationToken cancellationToken = default);

    Task ScrapeMovieImageAsync(string imdbCode, string source, string url, CancellationToken cancellationToken = default);
}

public class MassTransitScraperClient : IScraperClient
{
    private readonly IPublishEndpoint _publish;

    public MassTransitScraperClient(IPublishEndpoint publish)
    {
        _publish = publish;
    }

    public async Task NotifyDownloadCompleteAsync(string imdbCode, CancellationToken cancellationToken = default) =>
        await _publish.Publish(new NotifyDownloadComplete(imdbCode), cancellationToken);

    public async Task StartScrapeAsync(string scrapeId, CancellationToken cancellationToken = default) =>
        await _publish.Publish(new StartScrape(scrapeId), cancellationToken);

    public async Task ScrapeMovieImageAsync(string imdbCode, string source, string url, CancellationToken cancellationToken = default) =>
        await _publish.Publish(new ScrapeMovieImage(imdbCode, source, url), cancellationToken);
}
