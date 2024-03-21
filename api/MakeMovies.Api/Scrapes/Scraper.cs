using MakeMovies.Api.Movies;

namespace MakeMovies.Api.Scrapes;

public interface IScraper
{
    
    IAsyncEnumerable<Movie> ScrapeMoviesAsync(CancellationToken cancellationToken = default);
}