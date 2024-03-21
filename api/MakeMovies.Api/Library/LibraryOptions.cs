namespace MakeMovies.Api.Library;

public class LibraryOptions
{
    public JellyfinOptions? Jellyfin { get; set; }
    
    public string MovieLibraryPath { get; set; } = string.Empty;
    
    public string DownloadsPath { get; set; } = string.Empty;
}

public class JellyfinOptions
{
    public Uri? Url { get; set; }
    
    public string? ApiKey { get; set; }
}