namespace MakeMovies.Api.Movies;

public record Movie(
    string Id,
    string ImdbCode,
    string Title,
    string SearchableTitle,
    string Language,
    int Year,
    decimal Rating,
    TimeSpan Runtime,
    string? Description,
    ISet<string> Genres,
    string? YouTubeTrailerCode,
    DateTime DateCreated,
    IList<Torrent> Torrents,
    bool InLibrary = false
);

public record MovieSummary(
    string Id,
    string ImdbCode,
    string Title,
    int Year,
    decimal Rating,
    TimeSpan Runtime,
    string? Description,
    ISet<string> Genres,
    string? YouTubeTrailerCode,
    DateTime DateCreated,
    ISet<string> Quality,
    bool InLibrary);

public record Torrent(
    string Hash,
    string Quality,
    string Type,
    long SizeBytes,
    DateTime DateCreated
);

public enum SourceMovieField
{
    Title,
    DateCreated,
    Year
}

public static class MovieExtensions
{
    public static string CleanTitle(string s) => s.Trim().ToLowerInvariant().Normalize();
    
    public static MovieSummary Summary(this Movie movie) => new(movie.Id,
        movie.ImdbCode,
        movie.Title,
        movie.Year,
        movie.Rating,
        movie.Runtime,
        movie.Description,
        movie.Genres.ToHashSet(),
        movie.YouTubeTrailerCode,
        movie.DateCreated,
        movie.Torrents.Select(t => t.Quality).ToHashSet(),
        movie.InLibrary);
}