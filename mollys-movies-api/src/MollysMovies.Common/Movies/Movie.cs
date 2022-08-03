using MongoDB.Bson.Serialization.Attributes;

namespace MollysMovies.Common.Movies;

public class Movie
{
    public const string CollectionName = "movies";

    [BsonId]
    public string ImdbCode { get; set; } = string.Empty;

    public MovieMeta? Meta { get; set; }

    public List<Torrent> Torrents { get; set; } = new();

    public LocalMovieSource? LocalSource { get; set; }

    public MovieDownload? Download { get; set; }
}