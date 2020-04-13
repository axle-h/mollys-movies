using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MolliesMovies.Common.Routing;

namespace MolliesMovies.Movies
{
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
}