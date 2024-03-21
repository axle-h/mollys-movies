using MakeMovies.Api.Movies;
using Microsoft.AspNetCore.Mvc;

namespace MakeMovies.Api.Meta;

[ApiController]
[Route("api/v1")]
public class ImageController(IMovieRepository movieRepository, IMetaService metaService) : Controller
{
    [HttpGet("movie/{id}/image")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetAsync(id, cancellationToken);
        if (movie is null)
        {
            return NotFound();
        }
        var image = await metaService.GetImageAsync(movie.ImdbCode, cancellationToken);
        return image is null ? NotFound() : Redirect($"/movie-images/{image}"); // TODO permanent
    }
}