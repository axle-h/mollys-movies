namespace MakeMovies.Api.Scrapes;

public record Scrape(
    string Id,
    DateTime StartDate,
    DateTime? EndDate = null,
    bool? Success = null,
    int MovieCount = 0,
    int TorrentCount = 0,
    string? Error = null);

public enum ScrapeField
{
    StartDate
}