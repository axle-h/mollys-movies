using System;
using System.Collections.Generic;

namespace MollysMovies.Scraper.Yts.Models;

/// <summary>
///     Movie summary on the YTS service.
/// </summary>
/// <param name="Id">YTS ID of the movie. e.g. "38659".</param>
/// <param name="Url">YTS URL of the movie e.g. "https://yts.mx/movies/7-grandmasters-1977".</param>
/// <param name="ImdbCode">IMDB ID of the movie, used in IMDB movie URLs: imdb.com/title/{ImdbCode} e.g. "tt0182666".</param>
/// <param name="Title">Title of the movie e.g. "7 Grandmasters".</param>
/// <param name="TitleEnglish">English title of the movie e.g. "7 Grandmasters".</param>
/// <param name="TitleLong">Title of the movie with year e.g. "7 Grandmasters (1977)".</param>
/// <param name="Slug">Slugified title of the movie e.g. "7-grandmasters-1977".</param>
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
/// <param name="MpaRating">
///     Motion picture association film rating where applicable, when no trailer is available then this
///     is the empty string e.g. "M", see also https://en.wikipedia.org/wiki/Motion_Picture_Association_film_rating_system.
/// </param>
/// <param name="BackgroundImage">
///     Absolute YTS URL of the background image e.g.
///     "https://yts.mx/assets/images/movies/7_grandmasters_1977/background.jpg".
/// </param>
/// <param name="BackgroundImageOriginal">
///     Absolute YTS URL of the background image e.g.
///     "https://yts.mx/assets/images/movies/7_grandmasters_1977/background.jpg".
/// </param>
/// <param name="SmallCoverImage">
///     Absolute YTS URL of a small cover image e.g.
///     "https://yts.mx/assets/images/movies/7_grandmasters_1977/small-cover.jpg".
/// </param>
/// <param name="MediumCoverImage">
///     Absolute YTS URL of a medium cover image e.g.
///     "https://yts.mx/assets/images/movies/7_grandmasters_1977/medium-cover.jpg".
/// </param>
/// <param name="LargeCoverImage">
///     Absolute YTS URL of a large cover image e.g.
///     "https://yts.mx/assets/images/movies/7_grandmasters_1977/large-cover.jpg".
/// </param>
/// <param name="State">Aggregate torrent status e.g. "ok".</param>
/// <param name="Torrents">Torrents available for this movie.</param>
/// <param name="DateUploaded">Date and time the movie was uploaded to YTS e.g. "2021-12-20 14:59:27".</param>
/// <param name="DateUploadedUnix">Unix timestamp the movie was uploaded to YTS e.g. "1640008767".</param>
public record YtsMovieSummary(
    int Id,
    Uri Url,
    string ImdbCode,
    string Title,
    string TitleEnglish,
    string TitleLong,
    string Slug,
    int Year,
    decimal Rating,
    int Runtime,
    ICollection<string>? Genres,
    string Summary,
    string DescriptionFull,
    string Synopsis,
    string YtTrailerCode,
    string Language,
    string MpaRating,
    Uri BackgroundImage,
    Uri BackgroundImageOriginal,
    Uri SmallCoverImage,
    Uri MediumCoverImage,
    Uri LargeCoverImage,
    string State,
    ICollection<YtsTorrent>? Torrents,
    DateTime DateUploaded,
    long DateUploadedUnix
);