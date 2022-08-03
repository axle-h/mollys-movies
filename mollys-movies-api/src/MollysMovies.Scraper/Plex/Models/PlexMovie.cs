using System;

namespace MollysMovies.Scraper.Plex.Models;

public record PlexMovie(
    string ImdbCode,
    string Title,
    int Year,
    DateTime DateCreated,
    string ThumbPath
);