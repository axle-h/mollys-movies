namespace MakeMovies.Api.Meta;

public class MetaOptions
{
    public string ImagePath { get; set; } = "data/images";

    public TmdbOptions Tmdb { get; set; } = new();

    public OmdbOptions Omdb { get; set; } = new();
}

public class TmdbOptions
{
    public Uri? Url { get; set; }
    
    public string? AccessToken { get; set; }
}

public class OmdbOptions
{
    public Uri? Url { get; set; }
    
    public string? ApiKey { get; set; }
}