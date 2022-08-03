using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MollysMovies.Api.Common;
using MollysMovies.Api.Common.Routing;
using MollysMovies.Api.Movies.Models;
using MollysMovies.Api.Movies.Requests;

namespace MollysMovies.Api.Movies;

[PublicApiRoute]
public class MoviesController
{
    private readonly IMovieService _service;

    public MoviesController(IMovieService service)
    {
        _service = service;
    }

    [HttpGet("{imdbCode}")]
    public async Task<MovieDto> GetMovie(string imdbCode, CancellationToken cancellationToken = default) =>
        await _service.GetAsync(imdbCode, cancellationToken);

    [HttpGet]
    public async Task<PaginatedData<MovieDto>> SearchMovies(
        [FromQuery] SearchMoviesRequest request,
        CancellationToken cancellationToken = default) =>
        await _service.SearchAsync(request, cancellationToken);
}