namespace MollysMovies.ScraperClient;

public record StartScrape(string Id);

public record ScrapeSourceFailure(string Source, string Type, string Error);

public record NotifyScrapeComplete(string Id, List<ScrapeSourceFailure> Errors);

public record NotifyDownloadComplete(string ExternalId);

public record NotifyMovieAddedToLibrary(string ImdbCode);

public record ScrapeMovieImage(string ImdbCode, string Source, string Url);