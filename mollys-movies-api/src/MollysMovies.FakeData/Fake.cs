using System.Reflection;
using Bogus;
using MollysMovies.Common.Movies;
using MollysMovies.Common.Scraper;

namespace MollysMovies.FakeData;

public static class Fake
{
    public static readonly DateTime UtcNow = new(2021, 6, 10, 12, 30, 15, 100);

    public static Faker Faker { get; } = new();

    public static Faker<ScrapeSource> ScrapeSource { get; } = new Faker<ScrapeSource>()
        .StrictMode(true)
        .RuleFor(x => x.Source, f => f.SourceName())
        .RuleFor(x => x.Type, f => f.PickRandom<ScraperType>())
        .RuleFor(x => x.StartDate, f => f.Date.Past())
        .RuleFor(x => x.EndDate, () => null)
        .RuleFor(x => x.Success, f => f.Random.Bool())
        .RuleFor(x => x.Error, () => null)
        .RuleFor(x => x.MovieCount, f => f.Random.Number(0, 1000))
        .RuleFor(x => x.TorrentCount, f => f.Random.Number(0, 1000))
        .FinishWith((f, source) =>
        {
            if (f.Random.Bool())
            {
                source.EndDate = source.StartDate.Add(f.Date.Timespan(TimeSpan.FromHours(2)));
            }

            if (source.Success == false)
            {
                source.Error = f.Lorem.Sentence();
            }
        });

    public static Faker<Scrape> Scrape { get; } = new Faker<Scrape>()
        .StrictMode(true)
        .RuleFor(x => x.Id, f => f.MongoId())
        .RuleFor(x => x.StartDate, f => f.Date.Past())
        .RuleFor(x => x.EndDate, () => null)
        .RuleFor(x => x.Success, f => f.Random.Bool())
        .RuleFor(x => x.LocalMovieCount, f => f.Random.Number(0, 1000))
        .RuleFor(x => x.MovieCount, f => f.Random.Number(0, 1000))
        .RuleFor(x => x.TorrentCount, f => f.Random.Number(0, 1000))
        .RuleFor(x => x.Sources, () => ScrapeSource.Generate(2))
        .FinishWith((f, scrape) =>
        {
            if (f.Random.Bool())
            {
                scrape.EndDate = scrape.StartDate.Add(f.Date.Timespan(TimeSpan.FromHours(2)));
            }
        });

    public static Faker<Torrent> Torrent { get; } = new Faker<Torrent>()
        .RuleFor(x => x.Source, f => f.SourceName())
        .RuleFor(x => x.Url, f => f.Internet.Url())
        .RuleFor(x => x.Hash, f => f.TorrentHash())
        .RuleFor(x => x.Quality, f => f.MovieQuality())
        .RuleFor(x => x.Type, f => f.MovieType())
        .RuleFor(x => x.SizeBytes, f => f.Random.Long(104900000, 1074000000));

    public static Faker<LocalMovieSource> LocalMovieSource { get; } = new Faker<LocalMovieSource>()
        .RuleFor(x => x.Source, f => f.SourceName())
        .RuleFor(x => x.DateCreated, f => f.Date.Past())
        .RuleFor(x => x.DateScraped, f => f.Date.Past());

    public static Faker<MovieDownloadStatus> MovieDownloadStatus { get; } =
        new Faker<MovieDownloadStatus>()
            .RuleFor(x => x.Status, f => f.PickRandom<MovieDownloadStatusCode>())
            .RuleSet("Started", set => set.RuleFor(x => x.Status, MovieDownloadStatusCode.Started))
            .RuleSet("Downloaded", set => set.RuleFor(x => x.Status, MovieDownloadStatusCode.Downloaded))
            .RuleSet("Complete", set => set.RuleFor(x => x.Status, MovieDownloadStatusCode.Complete))
            .RuleFor(x => x.DateCreated, f => f.Date.Past());

    public static Faker<MovieDownload> MovieDownload { get; } = new Faker<MovieDownload>()
        .RuleFor(x => x.ExternalId, f => f.Id().ToString())
        .RuleFor(x => x.Name, f => f.MovieNameWithYear())
        .RuleFor(x => x.MagnetUri, f => f.Internet.Url())
        .RuleFor(x => x.Source, f => f.SourceName())
        .RuleFor(x => x.Quality, f => f.MovieQuality())
        .RuleFor(x => x.Type, f => f.MovieType())
        .RuleFor(x => x.Statuses, f => TransmissionContextStatuses(f).ToList())
        .RuleSet("Started",
            set => set.RuleFor(x => x.Statuses,
                f => TransmissionContextStatuses(f, MovieDownloadStatusCode.Started).ToList()))
        .RuleSet("Downloaded",
            set => set.RuleFor(x => x.Statuses,
                f => TransmissionContextStatuses(f, MovieDownloadStatusCode.Downloaded).ToList()))
        .RuleSet("Complete",
            set => set.RuleFor(x => x.Statuses,
                f => TransmissionContextStatuses(f, MovieDownloadStatusCode.Complete).ToList()));

    public static Faker<MovieMeta> MovieMeta { get; } = new Faker<MovieMeta>()
        .StrictMode(true)
        .RuleFor(x => x.Source, f => f.SourceName())
        .RuleFor(x => x.Title, f => f.MovieName())
        .RuleFor(x => x.Language, f => f.LanguageCode())
        .RuleFor(x => x.Year, f => f.MovieYear())
        .RuleFor(x => x.Rating, f => f.MovieRating())
        .RuleFor(x => x.Description, f => f.Lorem.Sentences(2))
        .RuleFor(x => x.YouTubeTrailerCode, f => f.YoutubeVideoId())
        .RuleFor(x => x.ImageFilename, f => f.System.FilePath())
        .RuleFor(x => x.Genres, f => f.Genres().ToHashSet())
        .RuleFor(x => x.RemoteImageUrl, f => f.Image.PicsumUrl(500, 750))
        .RuleFor(x => x.DateCreated, f => f.Date.Past())
        .RuleFor(x => x.DateScraped, f => f.Date.Past());

    public static Faker<Movie> Movie { get; } = new Faker<Movie>()
        .RuleFor(x => x.ImdbCode, f => f.ImdbCode())
        .RuleFor(x => x.Meta, () => MovieMeta.Generate())
        .RuleFor(x => x.LocalSource, () => LocalMovieSource.Generate())
        .RuleFor(x => x.Torrents, () => Torrent.Generate(2))
        .RuleFor(x => x.Download, () => MovieDownload.Generate("default,Complete"))
        .FinishWith((f, m) =>
        {
            var torrent = f.PickRandom(m.Torrents);
            m.Download!.Name = $"{m.Meta!.Title} ({m.Meta.Year})";
            m.Download.Quality = torrent.Quality;
            m.Download.Source = torrent.Source;
            m.Download.Type = torrent.Type;
        });

    public static string NicelyDatedString(string name) => DateTime.Now.ToString($"\"{name}\"-HH-mm-ss-FFFFFFF");

    public static string Resource(string resourceName)
    {
        var fullyQualifiedName = $"{typeof(Fake).Namespace}.{resourceName}";
        using var stream = Assembly.GetAssembly(typeof(Fake))!
                               .GetManifestResourceStream(fullyQualifiedName)
                           ?? throw new Exception($"cannot find resource {fullyQualifiedName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static IEnumerable<MovieDownloadStatus> TransmissionContextStatuses(Faker f,
        MovieDownloadStatusCode? lastStatus = null)
    {
        lastStatus ??= f.PickRandom<MovieDownloadStatusCode>();
        var date = f.Date.Past();
        for (var status = MovieDownloadStatusCode.Started; status <= lastStatus; status++)
        {
            var context = MovieDownloadStatus.Generate($"default,{status}");
            context.DateCreated = date;
            date = date.AddHours(1);
            yield return context;
        }
    }
}