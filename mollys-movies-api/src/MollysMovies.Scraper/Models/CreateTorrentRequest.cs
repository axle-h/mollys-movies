namespace MollysMovies.Scraper.Models;

/// <summary>
///     Request to create a new torrent.
/// </summary>
/// <param name="Url">Absolute URL of the .torrent file on the source service.</param>
/// <param name="Hash">BitTorrent info-hash of the torrent file. Used for magnet URI's.</param>
/// <param name="Quality">Quality of the rip.</param>
/// <param name="Type">Source of the rip.</param>
/// <param name="SizeBytes">Size of the file in bytes.</param>
public record CreateTorrentRequest(
    string Url,
    string Hash,
    string Quality,
    string Type,
    long SizeBytes
);