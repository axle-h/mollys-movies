namespace MakeMovies.Api.Scrapes;

public class ScrapeOptions
{
    public YtsOptions Yts { get; set; } = new();
    
    public Uri? ProxyUrl { get; set; }

    public ISet<string> Languages { get; set; } = new HashSet<string>();
}

public class YtsOptions
{
    public Uri? Url { get; set; }
    
    public TimeSpan RetryDelay { get; set; } = TimeSpan.Zero;
    
    public int MaxRetries { get; set; } = 50;
    
    public int Limit { get; set; } = 50;
}