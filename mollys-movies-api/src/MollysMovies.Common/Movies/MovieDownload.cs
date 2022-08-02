namespace MollysMovies.Common.Movies;

public class MovieDownload
{
    public string? ExternalId { get; set; }

    public string? Name { get; set; }

    public string? MagnetUri { get; set; }

    public string? Source { get; set; }

    public string? Quality { get; set; }

    public string? Type { get; set; }

    public List<MovieDownloadStatus> Statuses { get; set; } = new();
}