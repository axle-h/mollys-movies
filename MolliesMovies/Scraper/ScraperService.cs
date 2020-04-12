using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MolliesMovies.Common;
using MolliesMovies.Common.Data;
using MolliesMovies.Common.Exceptions;
using MolliesMovies.Movies;
using MolliesMovies.Movies.Data;
using MolliesMovies.Scraper.Data;
using MolliesMovies.Scraper.Models;
using MolliesMovies.Scraper.Plex;
using Polly;

namespace MolliesMovies.Scraper
{
    public class ScrapeResult
    {
        public int MovieCount { get; set; }
        
        public int TorrentCount { get; set; }
    }
    
    public interface IScraper
    {
        string Source { get; }
        
        ScraperType Type { get; }
        
        Task<ScrapeResult> ScrapeAsync(IScrapeSession session, CancellationToken cancellationToken = default);
    }

    public interface IScraperService
    {
        Task<ICollection<ScrapeDto>> GetAllAsync(CancellationToken cancellationToken = default);
        
        Task<ScrapeDto> CreateScrapeAsync(CancellationToken cancellationToken = default);
    }

    public interface IScraperInternalService
    {
        Task ScrapeAsync(int id, CancellationToken cancellationToken = default);

        Task UpdateLocalMovieLibrariesAsync(CancellationToken cancellationToken = default);
        
        Task<bool> ScrapeForLocalMovieAsync(int id, CancellationToken cancellationToken = default);
    }

    public class ScraperService : IScraperInternalService, IScraperService
    {
        private readonly ICollection<IScraper> _scrapers;
        private readonly ISystemClock _clock;
        private readonly MolliesMoviesContext _context;
        private readonly ILogger<ScraperService> _logger;
        private readonly IMapper _mapper;
        private readonly IMovieService _movieService;
        private readonly IPlexApiClient _plexApiClient;

        public ScraperService(
            IEnumerable<IScraper> scrapers,
            ISystemClock clock,
            ILogger<ScraperService> logger,
            IMapper mapper,
            MolliesMoviesContext context,
            IMovieService movieService,
            IPlexApiClient plexApiClient)
        {
            _clock = clock;
            _logger = logger;
            _mapper = mapper;
            _context = context;
            _movieService = movieService;
            _plexApiClient = plexApiClient;
            _scrapers = scrapers.ToList();
        }

        public async Task ScrapeAsync(int id, CancellationToken cancellationToken = default)
        {
            Scrape scrape;
            try
            {
                scrape = await _context.Scrapes().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (scrape is null)
                {
                    throw new Exception($"cannot find scrape record {id}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "failed when getting scrape record {id}", id);
                // TODO requeue on db fail
                return;
            }
            
            scrape.ScrapeSources = new List<ScrapeSource>();
            IScrapeSession session = null;
            foreach (var scraper in _scrapers)
            {
                var source = new ScrapeSource
                {
                    Source = scraper.Source,
                    Type = scraper.Type,
                    StartDate = _clock.UtcNow
                };
                scrape.ScrapeSources.Add(source);

                try
                {
                    session = await _movieService.GetScrapeSessionAsync(scraper.Source, scraper.Type, session, cancellationToken);
                    var result = await scraper.ScrapeAsync(session, cancellationToken);
                    source.Success = true;
                    source.EndDate = _clock.UtcNow;
                    source.MovieCount = result.MovieCount;
                    source.TorrentCount = result.TorrentCount;

                    switch (scraper.Type)
                    {
                        case ScraperType.Local:
                            scrape.LocalMovieCount += result.MovieCount;
                            break;
                        case ScraperType.Torrent:
                            scrape.MovieCount += result.MovieCount;
                            scrape.TorrentCount += result.TorrentCount;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "failed to scrape {source}", scraper.Source);
                    source.Success = false;
                    source.EndDate = _clock.UtcNow;
                    source.Error = e.ToString();
                }
            }

            scrape.Success = scrape.ScrapeSources.All(x => x.Success);
            scrape.EndDate = _clock.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<ICollection<ScrapeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var scrapes = await _context.Scrapes().ToListAsync(cancellationToken);
            return _mapper.Map<ICollection<ScrapeDto>>(scrapes);
        }

        public async Task<ScrapeDto> CreateScrapeAsync(CancellationToken cancellationToken = default)
        {
            var scrape = new Scrape
            {
                StartDate = _clock.UtcNow
            };
            
            _context.Add(scrape);
            await _context.SaveChangesAsync(cancellationToken);
            
            return _mapper.Map<ScrapeDto>(scrape);
        }

        public async Task UpdateLocalMovieLibrariesAsync(CancellationToken cancellationToken = default)
        {
            // TODO make non-plex specific
            var libraries = await _plexApiClient.GetMovieLibrariesAsync(cancellationToken);
            await Task.WhenAll(libraries.Select(x => _plexApiClient.UpdateLibraryAsync(x.Key, cancellationToken)));
        }
        
        public async Task<bool> ScrapeForLocalMovieAsync(int id, CancellationToken cancellationToken = default)
        {
            var movie = await _context.Set<Movie>().FindAsync(id);
            if (movie is null)
            {
                throw EntityNotFoundException.Of<Movie>(id);
            }
            
            IScrapeSession session = null;
            foreach (var scraper in _scrapers.Where(x => x.Type == ScraperType.Local))
            {
                session = await _movieService.GetScrapeSessionAsync(scraper.Source, scraper.Type, session, cancellationToken);
                await scraper.ScrapeAsync(session, cancellationToken);
                if (session.LocalImdbCodes.Contains(movie.ImdbCode))
                {
                    return true;
                }                
            }

            return false;
        }
    }
}