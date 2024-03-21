using MakeMovies.Api.Movies;
using Microsoft.AspNetCore.Mvc;

namespace MakeMovies.Api.Library;

[ApiController]
[Route("api/v1/library")]
[Produces("application/json")]
public class LibraryController(IMovieRepository movieRepository, ILibraryService libraryService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateLibraryMoviesAsync(CancellationToken cancellationToken = default)
    {
        var imdbCodesInLibrary = await libraryService.AllImdbCodesAsync(cancellationToken);
        await movieRepository.UpdateLibraryMoviesAsync(imdbCodesInLibrary, cancellationToken);
        return Ok();
    }
}