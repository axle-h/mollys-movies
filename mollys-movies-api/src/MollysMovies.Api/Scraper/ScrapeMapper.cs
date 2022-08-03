using System;
using System.Linq;
using MollysMovies.Api.Scraper.Models;
using MollysMovies.Common.Scraper;

namespace MollysMovies.Api.Scraper;

public interface IScrapeMapper
{
    ScrapeDto ToScrapeDto(Scrape scrape);

    ScrapeSourceDto ToScrapeSourceDto(ScrapeSource source);
}

public class ScrapeMapper : IScrapeMapper
{
    public ScrapeDto ToScrapeDto(Scrape scrape) => new(
        scrape.Id,
        scrape.StartDate,
        scrape.EndDate,
        scrape.Success,
        scrape.LocalMovieCount,
        scrape.MovieCount,
        scrape.TorrentCount,
        scrape.Sources.Select(ToScrapeSourceDto).ToList()
    );

    public ScrapeSourceDto ToScrapeSourceDto(ScrapeSource source) => new(
        source.Source ?? throw new ArgumentNullException(nameof(source), "source is required"),
        source.Type,
        source.Success,
        source.Error,
        source.StartDate,
        source.EndDate,
        source.MovieCount,
        source.TorrentCount
    );
}