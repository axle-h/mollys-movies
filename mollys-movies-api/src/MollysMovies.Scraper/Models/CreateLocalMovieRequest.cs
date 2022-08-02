using System;

namespace MollysMovies.Scraper.Models;

public record CreateLocalMovieRequest(string ImdbCode, string Title, int Year, DateTime DateCreated,
    string ThumbPath);