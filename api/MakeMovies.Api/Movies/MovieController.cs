using Microsoft.AspNetCore.Mvc;

namespace MakeMovies.Api.Movies;

[ApiController]
[Route("api/v1/movie")]
[Produces("application/json")]
public class MovieController(IMovieRepository repository) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Movie>> GetMovie(string id, CancellationToken cancellationToken = default)
    {
        var movie = await repository.GetAsync(id, cancellationToken);
        return movie is null ? NotFound() : Ok(movie);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<PaginatedData<MovieSummary>> ListAsync([FromQuery] PaginatedQuery<SourceMovieField> query,
        CancellationToken cancellationToken = default)
    {
        return repository.ListAsync(query, cancellationToken);
    }
}