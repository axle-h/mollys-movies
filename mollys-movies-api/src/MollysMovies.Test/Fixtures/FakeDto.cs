using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Bogus;
using Humanizer;
using MollysMovies.Api.Common;
using MollysMovies.Api.Movies.Models;
using MollysMovies.Api.Movies.Requests;
using MollysMovies.Api.Scraper.Models;
using MollysMovies.Api.Transmission.Models;
using MollysMovies.Common.Movies;
using MollysMovies.Common.Scraper;
using MollysMovies.Common.TransmissionRpc;
using MollysMovies.FakeData;
using MollysMovies.Scraper.Models;
using MollysMovies.Scraper.Plex.Models;
using MollysMovies.Scraper.Yts.Models;
using MollysMovies.ScraperClient;

namespace MollysMovies.Test.Fixtures;

public static class FakeDto
{
    public static Faker<SetDownloadRequest> SetDownloadRequest = new Faker<SetDownloadRequest>()
        .CustomInstantiator(f => new SetDownloadRequest(f.ImdbCode(), f.SourceName(), f.MovieQuality(),
            f.MovieType(), f.Random.Int().ToString(), f.MovieNameWithYear(), f.MagnetUri()));

    public static Faker<ScrapeSourceDto> ScrapeSourceDto { get; } = new Faker<ScrapeSourceDto>()
        .CustomInstantiator(f =>
        {
            var success = f.Random.Bool();
            var start = f.Date.Past();
            DateTime? end = f.Random.Bool() ? start.Add(f.Date.Timespan(TimeSpan.FromHours(2))) : null;
            return new ScrapeSourceDto(
                f.PickRandom("plex", "yts"),
                f.PickRandom<ScraperType>(),
                success,
                success ? null : f.Lorem.Sentence(),
                start,
                end,
                f.Random.Number(0, 1000),
                f.Random.Number(0, 1000)
            );
        });

    public static Faker<ScrapeDto> ScrapeDto { get; } = new Faker<ScrapeDto>()
        .CustomInstantiator(f =>
        {
            var start = f.Date.Past();
            DateTime? end = f.Random.Bool() ? start.Add(f.Date.Timespan(TimeSpan.FromHours(2))) : null;
            return new ScrapeDto(
                f.MongoId(),
                start,
                end,
                f.Random.Bool(),
                f.Random.Number(0, 1000),
                f.Random.Number(0, 1000),
                f.Random.Number(0, 1000),
                ScrapeSourceDto.Generate(2)
            );
        });

    public static Faker<YtsTorrent> YtsTorrent { get; } = new Faker<YtsTorrent>()
        .CustomInstantiator(f => new YtsTorrent(
            new Uri(f.Internet.Url()),
            f.TorrentHash(),
            f.PickRandom("720p", "1080p", "2160p", "3D"),
            f.PickRandom("bluray", "web"),
            f.SizeBytes().Bytes().Humanize(),
            f.SizeBytes(),
            f.Date.Past(),
            f.Date.PastOffset().ToUnixTimeSeconds()
        ));

    public static Faker<YtsMovieSummary> YtsMovieSummary { get; } = new Faker<YtsMovieSummary>()
        .CustomInstantiator(f => new YtsMovieSummary(
            f.Random.Number(1, int.MaxValue),
            new Uri(f.Internet.Url()),
            f.ImdbCode(),
            f.MovieName(),
            f.MovieName(),
            f.MovieName(),
            f.Random.Words().Kebaberize(),
            f.MovieYear(),
            f.MovieRating(),
            f.Random.Number(60, 240),
            f.Genres(),
            f.Lorem.Sentence(),
            f.Lorem.Sentences(2),
            f.Lorem.Paragraphs(2),
            f.YoutubeVideoId(),
            f.LanguageCode(),
            f.MpaaRating(),
            new Uri(f.Image.PicsumUrl(900, 400)),
            new Uri(f.Image.PicsumUrl(900, 400)),
            new Uri(f.Image.PicsumUrl(45, 67)),
            new Uri(f.Image.PicsumUrl(230, 345)),
            new Uri(f.Image.PicsumUrl(500, 750)),
            "ok",
            YtsTorrent.Generate(2),
            f.Date.Past(),
            f.Date.PastOffset().ToUnixTimeSeconds()
        ));

    public static Faker<CreateTorrentRequest> CreateTorrentRequest { get; } = new Faker<CreateTorrentRequest>()
        .CustomInstantiator(f => new CreateTorrentRequest(
            f.Internet.Url(),
            f.TorrentHash(),
            f.MovieQuality(),
            f.MovieType(),
            f.SizeBytes()
        ));

    public static Faker<CreateMovieRequest> CreateMovieRequest { get; } = new Faker<CreateMovieRequest>()
        .CustomInstantiator(f => new CreateMovieRequest(
            f.ImdbCode(),
            f.MovieName(),
            f.LanguageCode(),
            f.MovieYear(),
            f.MovieRating(),
            f.Lorem.Sentences(2),
            f.Genres(),
            f.YoutubeVideoId(),
            f.Image.PicsumUrl(500, 750),
            f.Internet.Url(),
            f.Random.Uuid().ToString(),
            f.Date.Past(),
            CreateTorrentRequest.Generate(2)
        ));

    public static Faker<CreateLocalMovieRequest> CreateLocalMovieRequest { get; } = new Faker<CreateLocalMovieRequest>()
        .CustomInstantiator(f => new CreateLocalMovieRequest(f.ImdbCode(), f.MovieName(),
            f.MovieYear(),
            f.Date.Past(), f.Internet.UrlRootedPath()));

    public static Faker<YtsImage> YtsImage { get; } = new Faker<YtsImage>()
        .CustomInstantiator(f => new YtsImage(f.Random.Bytes(8), f.System.MimeType()));


    public static Faker<MovieDownloadDto> MovieDownloadDto { get; } = new Faker<MovieDownloadDto>()
        .CustomInstantiator(f => new MovieDownloadDto(
            f.ImdbCode(),
            f.Id().ToString(), 
            f.MovieNameWithYear(), 
            f.PickRandom<MovieDownloadStatusCode>()));

    public static Faker<TorrentDto> TorrentDto { get; } = new Faker<TorrentDto>()
        .CustomInstantiator(f => new TorrentDto(
            f.SourceName(),
            f.Internet.Url(),
            f.TorrentHash(),
            f.MovieQuality(),
            f.MovieType(),
            f.SizeBytes()
        ));

    public static Faker<LocalMovieSourceDto> LocalMovieSourceDto { get; } = new Faker<LocalMovieSourceDto>()
        .CustomInstantiator(f => new LocalMovieSourceDto(
            f.SourceName(),
            f.Date.Past(),
            f.Date.Past()));

    public static Faker<MovieDto> MovieDto { get; } = new Faker<MovieDto>()
        .CustomInstantiator(f => new MovieDto(
            f.ImdbCode(),
            f.MovieName(),
            f.LanguageCode(),
            f.MovieYear(),
            f.MovieRating(),
            f.Lorem.Sentence(),
            f.YoutubeVideoId(),
            f.System.FileName(),
            f.Genres(),
            TorrentDto.Generate(2),
            LocalMovieSourceDto.Generate(),
            MovieDownloadDto.Generate()
        ));

    public static Faker<MovieImageSourceDto> MovieImageSourceDto { get; } = new Faker<MovieImageSourceDto>()
        .CustomInstantiator(f => new MovieImageSourceDto(f.SourceName(), f.Internet.Url()));

    public static Faker<MovieImageSourcesDto> MovieImageSourcesDto { get; } = new Faker<MovieImageSourcesDto>()
        .CustomInstantiator(f => new MovieImageSourcesDto(
            f.Id(),
            f.ImdbCode(),
            MovieImageSourceDto.Generate(),
            MovieImageSourceDto.Generate(1)
        ));

    public static Faker<ScrapeImageResult> ScrapeImageResult { get; } = new Faker<ScrapeImageResult>()
        .CustomInstantiator(f => new ScrapeImageResult(f.Random.Bytes(8), f.System.MimeType()));

    public static Faker<PlexMovie> PlexMovie { get; } = new Faker<PlexMovie>()
        .CustomInstantiator(f => new PlexMovie(f.ImdbCode(), f.MovieName(), f.MovieYear(),
            f.Date.Past(), f.Internet.UrlRootedPath()));

    public static Faker<PlexLibrary> PlexLibrary { get; } = new Faker<PlexLibrary>()
        .CustomInstantiator(f => new PlexLibrary(f.Random.Uuid().ToString(), "movie"));

    public static Faker<PlexMovieMetadata> PlexMovieMetadata { get; } = new Faker<PlexMovieMetadata>()
        .CustomInstantiator(f => new PlexMovieMetadata(f.Random.Uuid().ToString(), f.Date.Past()));

    public static Faker<PlexImage> PlexImage { get; } = new Faker<PlexImage>()
        .CustomInstantiator(f => new PlexImage(f.Random.Bytes(8), f.System.MimeType()));

    public static Faker<ScrapeSession> ScrapeSession { get; } = new Faker<ScrapeSession>()
        .CustomInstantiator(f => new ScrapeSession(f.Date.Past(), f.SourceName(), f.PickRandom<ScraperType>(), null));

    public static Faker<SearchMoviesRequest> SearchMoviesRequest { get; } = new Faker<SearchMoviesRequest>()
        .StrictMode(true)
        .RuleFor(x => x.Page, f => f.Random.Number(1, 1000))
        .RuleFor(x => x.Limit, f => f.Random.Number(1, 5) * 10)
        .RuleFor(x => x.Title, f => f.MovieName())
        .RuleFor(x => x.Quality, f => f.MovieQuality())
        .RuleFor(x => x.Language, f => f.LanguageCode())
        .RuleFor(x => x.HasDownload, f => f.Random.Bool())
        .RuleFor(x => x.Downloaded, f => f.Random.Bool())
        .RuleFor(x => x.Genre, f => f.Genre())
        .RuleFor(x => x.YearFrom, f => f.MovieYear())
        .RuleFor(x => x.YearTo, f => f.MovieYear())
        .RuleFor(x => x.RatingFrom, f => f.Random.Number(1, 5))
        .RuleFor(x => x.RatingTo, f => f.Random.Number(6, 10))
        .RuleFor(x => x.OrderBy, f => f.PickRandom<MoviesOrderBy>())
        .RuleFor(x => x.OrderByDescending, f => f.Random.Bool());

    public static Faker<DownloadMovieRequest> DownloadMovieRequest { get; } = new Faker<DownloadMovieRequest>()
        .CustomInstantiator(f => new DownloadMovieRequest(f.MovieNameWithYear(), f.MagnetUri()));

    public static Faker<GetLiveTransmissionStatusRequest> GetLiveTransmissionStatusRequest { get; } =
        new Faker<GetLiveTransmissionStatusRequest>()
            .CustomInstantiator(f => new GetLiveTransmissionStatusRequest(f.ImdbCode(), f.MovieNameWithYear()));

    public static Faker<LiveTransmissionStatusDto> LiveTransmissionStatusDto { get; } =
        new Faker<LiveTransmissionStatusDto>()
            .CustomInstantiator(f => new LiveTransmissionStatusDto(f.MovieNameWithYear(), f.Random.Bool(),
                f.Random.Bool(), f.Random.Number(0, 1000), f.Random.Double()));

    public static Faker<MockFileData> MockFileData { get; } = new Faker<MockFileData>()
        .CustomInstantiator(f => new MockFileData(f.Random.Bytes(8)));

    public static Faker<TorrentFile> TorrentFile { get; } = new Faker<TorrentFile>()
        .CustomInstantiator(f => new TorrentFile(f.System.FilePath(), f.SizeBytes(), f.SizeBytes()));

    public static Faker<TorrentInfo> TorrentInfo { get; } = new Faker<TorrentInfo>()
        .CustomInstantiator(f => new TorrentInfo(f.Id(), f.MovieNameWithYear(), f.Random.Double(), false,
            f.Random.Int(30, 300), f.System.DirectoryPath(), TorrentFile.Generate(2)));

    public static Faker<NewTorrentInfo> NewTorrentInfo { get; } = new Faker<NewTorrentInfo>()
        .CustomInstantiator(f => new NewTorrentInfo(f.Id(), f.MovieNameWithYear(), f.TorrentHash()));

    public static Faker<PaginatedMovieQuery> PaginatedMovieQuery { get; } = new Faker<PaginatedMovieQuery>()
        .StrictMode(true)
        .RuleFor(x => x.Downloaded, f => f.Random.Bool())
        .RuleFor(x => x.Genre, f => f.Genre())
        .RuleFor(x => x.Language, f => f.LanguageCode())
        .RuleFor(x => x.Quality, f => f.MovieQuality())
        .RuleFor(x => x.Text, f => f.MovieName())
        .RuleFor(x => x.HasDownload, f => f.Random.Bool())
        .RuleFor(x => x.RatingFrom, f => f.MovieRating())
        .RuleFor(x => x.RatingTo, f => f.MovieRating())
        .RuleFor(x => x.YearFrom, f => f.MovieYear())
        .RuleFor(x => x.YearTo, f => f.MovieYear())
        .RuleFor(x => x.Limit, f => f.Random.Int(1, 10) * 10)
        .RuleFor(x => x.Page, f => f.Random.Int(1, 10))
        .RuleFor(x => x.OrderBy, new List<PaginatedOrderBy<Movie>> {new(x => x.Meta!.Title, false)});

    public static Faker<ScrapeMovieImage> ScrapeMovieImage { get; } = new Faker<ScrapeMovieImage>()
        .CustomInstantiator(f => new ScrapeMovieImage(f.ImdbCode(), f.SourceName(), f.Internet.Url()));

    public static YtsResponseWrapper<TData> OkYtsWrapper<TData>(TData data) => new("ok", "Query was successful", data);

    public static YtsListMoviesResponse YtsListMoviesResponse(params YtsMovieSummary[] movies) =>
        new(movies.Length, 50, 1, movies);

    public static PaginatedData<T> PaginatedData<T>(ICollection<T> data) =>
        new()
        {
            Count = Fake.Faker.Random.Int(1, 10000),
            Data = data,
            Page = Fake.Faker.Random.Int(1, 10),
            Limit = Fake.Faker.Random.Int(1, 10) * 10
        };
}