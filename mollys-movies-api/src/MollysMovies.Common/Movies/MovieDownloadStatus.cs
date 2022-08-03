using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MollysMovies.Common.Movies;

public enum MovieDownloadStatusCode
{
    /// <summary>
    ///     The torrent has been added to the external transmission service.
    /// </summary>
    Started = 1,

    /// <summary>
    ///     The external transmission service has notified that the torrent is download.
    /// </summary>
    Downloaded = 2,

    /// <summary>
    ///     The movie is available in the local movie library.
    /// </summary>
    Complete = 3
}

public class MovieDownloadStatus
{
    [BsonRepresentation(BsonType.String)]
    public MovieDownloadStatusCode Status { get; set; }

    public DateTime DateCreated { get; set; }
}