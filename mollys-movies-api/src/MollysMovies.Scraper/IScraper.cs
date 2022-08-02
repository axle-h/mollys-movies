using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.Common.Scraper;
using MollysMovies.Scraper.Models;

namespace MollysMovies.Scraper;

public interface IScraper
{
    string Source { get; }

    ScraperType Type { get; }

    Task<ScrapeImageResult> ScrapeImageAsync(string url, CancellationToken cancellationToken = default);

    Task HealthCheckAsync(CancellationToken cancellationToken = default);
}

public interface ITorrentScraper : IScraper
{
    IAsyncEnumerable<CreateMovieRequest> ScrapeMoviesAsync(ScrapeSession session,
        CancellationToken cancellationToken = default);
}

public interface ILocalScraper : IScraper
{
    IAsyncEnumerable<CreateLocalMovieRequest> ScrapeMoviesAsync(ScrapeSession session,
        CancellationToken cancellationToken = default);

    Task UpdateMovieLibrariesAsync(CancellationToken cancellationToken = default);
}

public static class ScraperServiceCollectionExtensions
{
    public static IServiceCollection AddTorrentScraper<TScraper>(this IServiceCollection services)
        where TScraper : class, ITorrentScraper =>
        services.AddTransient<IScraper, TScraper>().AddTransient<ITorrentScraper, TScraper>();

    public static IServiceCollection AddLocalScraper<TScraper>(this IServiceCollection services)
        where TScraper : class, ILocalScraper =>
        services.AddTransient<IScraper, TScraper>().AddTransient<ILocalScraper, TScraper>();
}