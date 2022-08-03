namespace MollysMovies.Common.Movies;

public class MovieMeta
{
    public string? Source { get; set; }

    public string? Title { get; set; }

    public string? Language { get; set; }

    public int Year { get; set; }

    public decimal? Rating { get; set; }

    public string? Description { get; set; }

    public string? YouTubeTrailerCode { get; set; }

    public string? ImageFilename { get; set; }

    public HashSet<string> Genres { get; set; } = new();

    public string? RemoteImageUrl { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateScraped { get; set; }
}