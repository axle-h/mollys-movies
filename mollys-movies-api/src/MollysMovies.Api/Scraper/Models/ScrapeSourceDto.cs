using System;
using MollysMovies.Common.Scraper;

namespace MollysMovies.Api.Scraper.Models;

public record ScrapeSourceDto(
    string Source,
    ScraperType Type,
    bool? Success,
    string? Error,
    DateTime StartDate,
    DateTime? EndDate,
    int MovieCount,
    int TorrentCount
);