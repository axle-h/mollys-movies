using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MollysMovies.Api.Common.Routing;

namespace MollysMovies.Api.Movies;

[PublicApiRoute]
public class GenreController : ControllerBase
{
    private readonly IMovieService _service;

    public GenreController(IMovieService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ICollection<string>> GetAllGenres(CancellationToken cancellationToken = default) =>
        await _service.GetAllGenresAsync(cancellationToken);
}