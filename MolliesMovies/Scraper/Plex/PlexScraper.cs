using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MolliesMovies.Common.Data;
using MolliesMovies.Movies;
using MolliesMovies.Movies.Data;
using MolliesMovies.Movies.Models;
using MolliesMovies.Movies.Requests;
using MolliesMovies.Scraper.Data;

namespace MolliesMovies.Scraper.Plex
{
    public class PlexScraper : IScraper
    {
        private readonly IPlexApiClient _client;
        private readonly ILogger<PlexScraper> _logger;
        private readonly IMapper _mapper;

        public PlexScraper(IPlexApiClient client, ILogger<PlexScraper> logger, IMapper mapper)
        {
            _client = client;
            _logger = logger;
            _mapper = mapper;
        }

        public string Source { get; } = "plex";

        public ScraperType Type { get; } = ScraperType.Local;
        
        public async Task<ScrapeResult> ScrapeAsync(IScrapeSession session, CancellationToken cancellationToken)
        {
            var libraries = await _client.GetMovieLibrariesAsync(cancellationToken);
            if (!libraries.Any())
            {
                throw new Exception("no plex movie libraries found");
            }
            
            _logger.LogInformation("found {libraryCount} libraries", libraries.Count);

            var movieTasks = libraries.Select(x => _client.GetMoviesAsync(x.Key, cancellationToken));
            var movies = (await Task.WhenAll(movieTasks)).SelectMany(x => x).ToList();

            _logger.LogInformation("found {moviesCount} movies", movies.Count);
            
            var newMovies = session.ScrapeFrom.HasValue
                ? movies.Where(x => x.DateCreated > session.ScrapeFrom.Value).ToList()
                : movies;

            var requests = _mapper.Map<ICollection<CreateLocalMovieRequest>>(newMovies);
            await session.CreateLocalMoviesAsync(requests);

            return new ScrapeResult
            {
                MovieCount = newMovies.Count
            };
        }

        public async Task<CreateMovieImageRequest> ScrapeImageAsync(string imDbCode, MovieImageSourceDto source, CancellationToken cancellationToken = default)
        {
            var image = await _client.GetThumbAsync(source.Value, cancellationToken);
            return new CreateMovieImageRequest
            {
                Content = image.Content,
                ContentType = image.ContentType,
                ImdbCode = imDbCode
            };
        }
    }
}