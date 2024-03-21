using Microsoft.AspNetCore.Mvc;

namespace MakeMovies.Api.Movies;

[ApiController]
[Route("api/v1/genre")]
[Produces("application/json")]
public class GenreController(IMovieRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ISet<string>> GetAllGenresAsync(CancellationToken cancellationToken = default) =>
        await repository.GetAllGenresAsync(cancellationToken);
}