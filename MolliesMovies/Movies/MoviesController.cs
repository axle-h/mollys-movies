using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MolliesMovies.Common;
using MolliesMovies.Common.Routing;
using MolliesMovies.Movies.Models;
using MolliesMovies.Movies.Requests;

namespace MolliesMovies.Movies
{
    [PublicApiRoute]
    public class MoviesController
    {
        private readonly IMovieService _service;

        public MoviesController(IMovieService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<MovieDto> GetMovie(int id, CancellationToken cancellationToken = default) =>
            await _service.GetAsync(id, cancellationToken);

        [HttpGet]
        public async Task<Paginated<MovieDto>> SearchMovies(
            [FromQuery] SearchMoviesRequest request,
            CancellationToken cancellationToken = default) =>
            await _service.SearchAsync(request, cancellationToken);
    }
}