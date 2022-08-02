using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MollysMovies.Common.Scraper;

public class Scrape
{
    public const string CollectionName = "scrapes";

    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    public string Id { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? Success { get; set; }

    public int LocalMovieCount { get; set; }

    public int MovieCount { get; set; }

    public int TorrentCount { get; set; }

    public ICollection<ScrapeSource> Sources { get; set; } = new List<ScrapeSource>();
}