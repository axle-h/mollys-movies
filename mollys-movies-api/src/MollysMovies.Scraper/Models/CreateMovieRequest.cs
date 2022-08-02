using System;
using System.Collections.Generic;

namespace MollysMovies.Scraper.Models;

/// <summary>
///     Request to create a new movie.
/// </summary>
/// <param name="ImdbCode">IMDB ID of the movie.</param>
/// <param name="Title">Title of the movie.</param>
/// <param name="Language">Language code of the primary spoken language.</param>
/// <param name="Year">Year of release.</param>
/// <param name="Rating">Rating from 0 to 10.</param>
/// <param name="Description">Long description of the movie.</param>
/// <param name="Genres">Genres of the movie.</param>
/// <param name="YouTubeTrailerCode">Youtube video ID of trailer.</param>
/// <param name="SourceCoverImageUrl">Relative URL to the cover image on the source service.</param>
/// <param name="SourceUrl">Relative URL to the movie page on the source service.</param>
/// <param name="SourceId">ID of the movie on the source service.</param>
/// <param name="DateCreated">Date this movie was created on the source service.</param>
/// <param name="Torrents">All torrents available on the source service.</param>
public record CreateMovieRequest(
    string ImdbCode,
    string Title,
    string Language,
    int Year,
    decimal Rating,
    string? Description,
    ICollection<string> Genres,
    string? YouTubeTrailerCode,
    string SourceCoverImageUrl,
    string SourceUrl,
    string SourceId,
    DateTime DateCreated,
    ICollection<CreateTorrentRequest> Torrents
);