using MollysMovies.Scraper.Models;
using MollysMovies.Scraper.Plex.Models;

namespace MollysMovies.Scraper.Plex;

public interface IPlexMapper
{
    CreateLocalMovieRequest ToCreateLocalMovieRequest(PlexMovie movie);
}

public class PlexMapper : IPlexMapper
{
    public CreateLocalMovieRequest ToCreateLocalMovieRequest(PlexMovie movie) => new(
        movie.ImdbCode,
        movie.Title,
        movie.Year,
        movie.DateCreated,
        movie.ThumbPath
    );
}