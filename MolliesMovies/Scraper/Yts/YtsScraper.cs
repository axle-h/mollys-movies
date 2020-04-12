using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MolliesMovies.Movies;
using MolliesMovies.Movies.Requests;
using MolliesMovies.Scraper.Data;
using MolliesMovies.Scraper.Yts.Models;

namespace MolliesMovies.Scraper.Yts
{
    public class YtsScraper : IScraper
    {
        private readonly IYtsClient _client;
        private readonly ILogger<YtsScraper> _logger;
        private readonly IOptions<ScraperOptions> _options;
        private readonly IMapper _mapper;

        public YtsScraper(
            IYtsClient client,
            ILogger<YtsScraper> logger,
            IOptions<ScraperOptions> options,
            IMapper mapper)
        {
            _client = client;
            _logger = logger;
            _options = options;
            _mapper = mapper;
        }

        public string Source { get; } = "yts";
        
        public ScraperType Type { get; } = ScraperType.Torrent;

        public async Task<ScrapeResult> ScrapeAsync(IScrapeSession session, CancellationToken cancellationToken = default)
        {
            var result = new ScrapeResult();
            var options = _options.Value.Yts;
            
            _logger.LogInformation("Scraping yts movies from {fromDate}", session.ScrapeFrom);
            
            for (var page = 1; page < int.MaxValue; page++)
            {
                _logger.LogInformation("scraping page {page}", page);
                var request = new YtsListMoviesRequest
                {
                    Page = page,
                    Limit = 50,
                    OrderBy = "desc",
                    SortBy = "date_added"
                };
                var response = await _client.ListMoviesAsync(request, cancellationToken);

                var movies = session.ScrapeFrom.HasValue
                    ? response.Movies
                        .Where(x => x.DateUploaded >= session.ScrapeFrom.Value)
                        .ToList()
                    : response.Movies;
                
                if (!movies.Any())
                {
                    break;
                }
                
                _logger.LogInformation("retrieved {movieCount} movies", movies.Count);

                var requests = _mapper.Map<ICollection<CreateMovieRequest>>(movies);

                await session.CreateMoviesAsync(requests);

                _logger.LogInformation("added {movieCount} movies", requests.Count);

                result.MovieCount += requests.Count;
                result.TorrentCount += requests.Sum(x => x.Torrents?.Count ?? 0);
                
                if (movies.Count < request.Limit)
                {
                    break;
                }

                if (options.ScrapeDelay > TimeSpan.Zero)
                {
                    await Task.Delay(options.ScrapeDelay, cancellationToken);
                }
            }

            _logger.LogInformation("done");

            return result;
        }
    }
}