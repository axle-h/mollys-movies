using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MolliesMovies.Common;
using MolliesMovies.Movies.Models;
using MolliesMovies.Movies.Requests;

namespace MolliesMovies.Movies
{
    [Route("/api/movies")]
    public class MoviesController
    {
        private readonly IMovieService _service;

        public MoviesController(IMovieService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<Paginated<MovieDto>> Search(
            [FromQuery] SearchMoviesRequest request,
            CancellationToken cancellationToken = default) =>
            await _service.SearchAsync(request, cancellationToken);
    }
}