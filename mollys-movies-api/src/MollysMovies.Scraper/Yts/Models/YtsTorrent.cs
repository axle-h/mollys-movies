using System;

namespace MollysMovies.Scraper.Yts.Models;

/// <summary>
///     Torrent on the YTS service.
/// </summary>
/// <param name="Url">
///     URL of the .torrent file on YTS e.g.
///     "https://yts.mx/torrent/download/4221BA07DFAB47E0F5B54ADB2935EEA405E48D12".
/// </param>
/// <param name="Hash">
///     BitTorrent info-hash of the torrent file. Used for magnet URI's.
///     e.g. "4221BA07DFAB47E0F5B54ADB2935EEA405E48D12".
/// </param>
/// <param name="Quality">Quality of the rip e.g. "720p".</param>
/// <param name="Type">Source type of the rip e.g. "bluray".</param>
/// <param name="Size">Human readable size of the file e.g. "815.8 MB".</param>
/// <param name="SizeBytes">Size of the file in bytes e.g. "855428301".</param>
/// <param name="DateUploaded">Date and time the torrent was uploaded to YTS0 e.g. "2021-12-20 14:59:27".</param>
/// <param name="DateUploadedUnix">Unix timestamp the torrent was uploaded to YTS e.g. "1640008767".</param>
public record YtsTorrent(
    Uri Url,
    string Hash,
    string Quality,
    string Type,
    string Size,
    long SizeBytes,
    DateTime DateUploaded,
    long DateUploadedUnix
);