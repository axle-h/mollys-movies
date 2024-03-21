namespace MakeMovies.Api.Downloads;

public class DownloadOptions
{
    public Transmission Transmission { get; set; } = new();

    public IList<string> PreferredQuality { get; set; } = [];
    
    public IList<string> PreferredType { get; set; } = [];

    public IList<string> Trackers { get; set; } = [];
    
    public TimeSpan BackgroundJobPeriod { get; set; } = TimeSpan.FromSeconds(15);
    
    public TimeSpan DownloadGracePeriod { get; set; } = TimeSpan.FromSeconds(15);
}

public class Transmission
{
    public Uri? Url { get; set; }
}