using System;
using MollysMovies.Common.Scraper;

namespace MollysMovies.Scraper.Models;

public record ScrapeSession(
    DateTime ScrapeDate,
    string Source,
    ScraperType Type,
    DateTime? ScrapeFrom);