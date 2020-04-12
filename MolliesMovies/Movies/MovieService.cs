using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MolliesMovies.Common;
using MolliesMovies.Common.Data;
using MolliesMovies.Common.Exceptions;
using MolliesMovies.Movies.Data;
using MolliesMovies.Movies.Models;
using MolliesMovies.Movies.Requests;
using MolliesMovies.Scraper.Data;

namespace MolliesMovies.Movies
{
    public interface IMovieService
    {
        Task<Paginated<MovieDto>> SearchAsync(
            SearchMoviesRequest request,
            CancellationToken cancellationToken = default);

        Task<IScrapeSession> GetScrapeSessionAsync(string source, ScraperType type, IScrapeSession lastSession = null, CancellationToken cancellationToken = default);

        Task<MovieDto> GetAsync(int id, CancellationToken cancellationToken = default);
    }

    public class MovieService : IMovieService
    {
        private readonly MolliesMoviesContext _context;
        private readonly IMapper _mapper;
        private readonly ISystemClock _clock;

        public MovieService(IMapper mapper, MolliesMoviesContext context, ISystemClock clock)
        {
            _mapper = mapper;
            _context = context;
            _clock = clock;
        }

        public async Task<IScrapeSession> GetScrapeSessionAsync(string source, ScraperType type, IScrapeSession lastSession = null, CancellationToken cancellationToken = default)
        {
            var scrapeFrom = type == ScraperType.Local
                ? (await _context.GetLatestLocalMovieBySourceAsync(source, cancellationToken))?.DateCreated
                : (await _context.GetLatestMovieBySourceAsync(source, cancellationToken))?.DateCreated;

            var movieImdbCodes = lastSession?.MovieImdbCodes ?? await _context.Set<Movie>().Select(x => x.ImdbCode).ToListAsync(cancellationToken);
            var localImdbCodes = lastSession?.LocalImdbCodes ?? await _context.Set<LocalMovie>().Select(x => x.ImdbCode).ToListAsync(cancellationToken);
            
            return new ScrapeSession(source, type, scrapeFrom, movieImdbCodes, localImdbCodes, _context, _clock, cancellationToken);
        }

        public async Task<MovieDto> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var movie = await _context.GetMovieDetailByIdAsync(id, cancellationToken);
            if (movie is null)
            {
                throw EntityNotFoundException.Of<Movie>(id);
            }

            return _mapper.Map<MovieDto>(movie);
        }

        public async Task<Paginated<MovieDto>> SearchAsync(
            SearchMoviesRequest request,
            CancellationToken cancellationToken = default)
        {
            PaginatedOrderBy<Movie> OrderBy(Expression<Func<Movie, object>> property) =>
                new PaginatedOrderBy<Movie> { Property = property, Descending = request.OrderByDescending ?? false };
            
            var query = new PaginatedMovieQuery
            {
                Page = request.Page ?? 1,
                Limit = request.Limit ?? 20,
                Title = request.Title,
                Quality = request.Quality,
                Language = request.Language,
                Downloaded = request.Downloaded,
                OrderBy = (request.OrderBy ?? MoviesOrderBy.Title) switch
                {
                    MoviesOrderBy.Title => new[] { OrderBy(x => x.Title) },
                    MoviesOrderBy.Year => new[] { OrderBy(x => x.Year), OrderBy(x => x.Title) },
                    MoviesOrderBy.Rating => new[] { OrderBy(x => x.Rating) },
                    _ => throw new ArgumentOutOfRangeException()
                },
            };

            var result = await _context.SearchMoviesAsync(query, cancellationToken);
            return _mapper.Map<Paginated<MovieDto>>(result);
        }
    }
}