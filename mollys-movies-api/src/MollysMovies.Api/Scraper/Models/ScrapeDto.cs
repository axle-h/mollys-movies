using System;
using System.Collections.Generic;

namespace MollysMovies.Api.Scraper.Models;

public record ScrapeDto(
    string Id,
    DateTime StartDate,
    DateTime? EndDate,
    bool? Success,
    int LocalMovieCount,
    int MovieCount,
    int TorrentCount,
    ICollection<ScrapeSourceDto> Sources
);