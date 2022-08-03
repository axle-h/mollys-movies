using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MollysMovies.Common.Scraper;
using MollysMovies.Scraper.Movies;
using MollysMovies.ScraperClient;

namespace MollysMovies.Scraper.Image;

public interface IImageScraperService
{
    Task ScrapeImageAsync(ScrapeMovieImage request, CancellationToken cancellationToken = default);
}

public class ImageScraperService : IImageScraperService
{
    private readonly IMovieImageRepository _imageRepository;
    private readonly ILogger<ImageScraperService> _logger;
    private readonly IMovieRepository _movieRepository;
    private readonly List<ITorrentScraper> _scrapers;

    public ImageScraperService(
        ILogger<ImageScraperService> logger,
        IMovieImageRepository imageRepository,
        IEnumerable<ITorrentScraper> scrapers,
        IMovieRepository movieRepository)
    {
        _logger = logger;
        _imageRepository = imageRepository;
        _movieRepository = movieRepository;
        _scrapers = scrapers.ToList();
    }

    public async Task ScrapeImageAsync(ScrapeMovieImage request, CancellationToken cancellationToken = default)
    {
        var path = await GetImagePathAsync(request, cancellationToken);
        await _movieRepository.UpdateMovieImageAsync(request.ImdbCode, path, cancellationToken);
    }

    private async Task<string> GetImagePathAsync(ScrapeMovieImage request, CancellationToken cancellationToken)
    {
        var existingImage = _imageRepository.GetMovieImage(request.ImdbCode);
        if (existingImage is not null)
        {
            _logger.LogInformation("no need to scrape image as already exists {ImdbCode}", request.ImdbCode);
            return existingImage;
        }

        var scraper = _scrapers.FirstOrDefault(x => x.Source == request.Source)
                      ?? throw new Exception($"no scraper registered for source {request.Source}");

        var (content, contentType) = await scraper.ScrapeImageAsync(request.Url, cancellationToken);
        _logger.LogInformation("successfully scraped image for imdb code '{ImdbCode}' from '{Source}'", request.ImdbCode, scraper.Source);
        return await _imageRepository.CreateMovieImageAsync(request.ImdbCode, content, contentType, cancellationToken);
    }
}