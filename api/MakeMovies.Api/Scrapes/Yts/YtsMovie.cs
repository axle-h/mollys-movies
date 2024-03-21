namespace MakeMovies.Api.Scrapes.Yts;


/// <summary>
///     Movie summary on the YTS service.
/// </summary>
/// <param name="Id">YTS ID of the movie. e.g. "38659".</param>
/// <param name="ImdbCode">IMDB ID of the movie, used in IMDB movie URLs: imdb.com/title/{ImdbCode} e.g. "tt0182666".</param>
/// <param name="Title">Title of the movie e.g. "7 Grandmasters".</param>
/// <param name="TitleEnglish">English title of the movie e.g. "7 Grandmasters".</param>
/// <param name="Year">Year of release e.g. "1977".</param>
/// <param name="Rating">Rating from 0 to 10 e.g. "7.1".</param>
/// <param name="Runtime">Runtime in minutes e.g. "89".</param>
/// <param name="Genres">Genres of the movie e.g. "Action, Comedy, Drama".</param>
/// <param name="Summary">
///     Short description of the movie e.g.
///     "An aging martial arts expert is gifted a plaque from the Emperor declaring him the Kung Fu World Champion.
///     Unsure of whether or not be is deserving of this title, he embarks on a journey to defeat the 7 Grandmasters."
/// </param>
/// <param name="DescriptionFull">
///     Long description of the movie e.g.
///     "An aging martial arts expert is gifted a plaque from the Emperor declaring him the Kung Fu World Champion.
///     Unsure of whether or not be is deserving of this title, he embarks on a journey to defeat the 7 Grandmasters."
/// </param>
/// <param name="Synopsis">
///     Full synopsis of the movie e.g.
///     "An aging martial arts expert is gifted a plaque from the Emperor declaring him the Kung Fu World Champion.
///     Unsure of whether or not be is deserving of this title, he embarks on a journey to defeat the 7 Grandmasters."
/// </param>
/// <param name="YtTrailerCode">
///     Youtube video ID of trailer, when no trailer is available then this is the empty string
///     e.g. "kgkmuq-o48E".
/// </param>
/// <param name="Language">
///     Language code of the primary spoken language e.g. "en", see also
///     http://www.lingoes.net/en/translator/langcode.htm.
/// </param>
/// <param name="Torrents">Torrents available for this movie.</param>
/// <param name="DateUploadedUnix">Unix timestamp the movie was uploaded to YTS e.g. "1640008767".</param>
public record YtsMovie(
    int Id,
    string ImdbCode,
    string Title,
    string? TitleEnglish,
    int Year,
    decimal Rating,
    int Runtime,
    ISet<string>? Genres,
    string Summary,
    string DescriptionFull,
    string Synopsis,
    string YtTrailerCode,
    string Language,
    ICollection<YtsTorrent>? Torrents,
    long DateUploadedUnix
);

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
/// <param name="SizeBytes">Size of the file in bytes e.g. "855428301".</param>
/// <param name="DateUploadedUnix">Unix timestamp the torrent was uploaded to YTS e.g. "1640008767".</param>
public record YtsTorrent(
    string Hash,
    string Quality,
    string Type,
    long SizeBytes,
    long DateUploadedUnix
);