using System.Runtime.CompilerServices;
using MakeMovies.Api.Library;
using MakeMovies.Api.Movies;

namespace MakeMovies.Api.Scrapes;

public interface IScrapeService
{
    Task<Scrape?> StartScrapeAsync(CancellationToken cancellationToken = default);
    
    Task<PaginatedData<Scrape>> ListAsync(PaginatedQuery<ScrapeField> query, CancellationToken cancellationToken = default);
}

public class ScrapeService(
    ILogger<ScrapeService> logger,
    IRootScraper scraper,
    IMovieRepository movieRepository,
    IScrapeRepository scrapeRepository,
    ILibraryService libraryService) : IScrapeService
{
    private readonly SemaphoreSlim _semaphore = new(1);
    
    public async Task<Scrape?> StartScrapeAsync(CancellationToken cancellationToken = default)
    {
        if (_semaphore.CurrentCount == 0)
        {
            return null;
        }

        var scrape = await scrapeRepository.NewAsync(cancellationToken);

        var _ = Task.Run( () => RunScrapeAsync(scrape), CancellationToken.None);

        return scrape;
    }

    private async Task RunScrapeAsync(Scrape scrape)
    {
        await _semaphore.WaitAsync();
        try
        {
            var movies = MergeWithLibraryAsync(scraper.ScrapeMoviesAsync());
            var stats = await movieRepository.WriteScrapedAsync(movies);
            scrape = scrape with { Success = true, EndDate = DateTime.UtcNow, MovieCount = stats.Movies, TorrentCount = stats.Torrents };
            logger.LogInformation("scrape complete");
        }
        catch (Exception e)
        {
            scrape = scrape with { Success = false, EndDate = DateTime.UtcNow, Error = e.ToString() };
            logger.LogError(e, "scrape failed");
        }
        finally
        {
            _semaphore.Release();
        }

        try
        {
            await scrapeRepository.UpdateAsync(scrape);
        }
        catch (Exception e)
        {
            logger.LogError(e, "failed to update scrape");
        }
    }

    private async IAsyncEnumerable<Movie> MergeWithLibraryAsync(
        IAsyncEnumerable<Movie> movies,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var libraryImdbCodes = await libraryService.AllImdbCodesAsync(cancellationToken);
        await foreach (var movie in movies.WithCancellation(cancellationToken))
        {
            if (libraryImdbCodes.Contains(movie.ImdbCode))
            {
                yield return movie with { InLibrary = true };
            }
            else
            {
                yield return movie;
            }
        }
    }

    public Task<PaginatedData<Scrape>> ListAsync(PaginatedQuery<ScrapeField> query,
        CancellationToken cancellationToken = default) =>
        scrapeRepository.ListAsync(query, cancellationToken);
}