using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MollysMovies.Common.Scraper;

public class ScrapeSource
{
    public string? Source { get; set; }

    [BsonRepresentation(BsonType.String)]
    public ScraperType Type { get; set; }

    public bool? Success { get; set; }

    public string? Error { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int MovieCount { get; set; }

    public int TorrentCount { get; set; }
}