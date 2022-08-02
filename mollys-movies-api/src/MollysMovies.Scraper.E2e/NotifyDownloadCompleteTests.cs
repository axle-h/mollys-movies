using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MassTransit;
using MollysMovies.Common.Movies;
using MollysMovies.FakeData.FileSystem;
using MollysMovies.FakeData;
using MollysMovies.Scraper.Plex.Models;
using MollysMovies.ScraperClient;
using MongoDB.Driver;
using Xunit;

namespace MollysMovies.Scraper.E2e;

[Collection("Scraper Tests")]
public class NotifyDownloadCompleteTests
{
    [Fact]
    public async Task Completes_download()
    {
        await using var fixture = new MollysMoviesScraperFixture("completes-download");
        using var rabbitMq = await fixture.RabbitMq()
            .Consume<NotifyMovieAddedToLibrary>()
            .RunAsync();
        var download = Fake.MovieDownload
            .With(c =>
            {
                c.Name = "Back to the Future Part III (1990)";
                c.ExternalId = "123456789";
            })
            .Generate("default,Started");
        var movie = Fake.Movie.With(m =>
        {
            m.Meta!.Title = "Back to the Future Part III";
            m.Meta!.Year = 1990;
            m.LocalSource = null;
            m.Download = download;
        }).Generate();
        await fixture.AddMoviesAsync(movie);
        
        const string transmissionName = "Back to the Future Part III (1990) [YTS]";
        var files = new Dictionary<string, string>
        {
            ["back.to.the.future.part.III.h264.1080p.yts.mp4"] = "movie",
            ["Subs/English.srt"] = "subs",
            ["YTS.txt"] = "junk"
        };

        foreach (var (filename, content) in files)
        {
            fixture.FileSystem.AddFile($"/var/downloads/{transmissionName}/{filename}",
                new MockFileData(content));
        }

        var torrentRemoveUuid = Guid.NewGuid();
        var date = DateTime.UtcNow;
        fixture.WireMock
            .GivenTransmissionRpc("torrent-get", new {ids = new[] {123456789}}, new
            {
                torrents = new[]
                {
                    new
                    {
                        id = 123456789, percentDone = 1, isStalled = false, eta = -1, downloadDir = "/var/downloads",
                        name = "Back to the Future Part III (1990) [YTS]",
                        files = files.Keys.Select(x => new {name = $"{transmissionName}/{x}"})
                    }
                }
            })
            .GivenTransmissionRpc("torrent-remove", new {ids = new[] {123456789}}, new { }, torrentRemoveUuid)
            .GivenPlexUpdateMovieLibrary()
            .GivenPlexLibraries()
            .GivenPlexMovieLibrary(new PlexMovieMetadata("100", date))
            .GivenPlexMovieMetadata("100", date, movie.ImdbCode, movie.Meta!.Title!, movie.Meta.Year);

        var message = new NotifyDownloadComplete("123456789");
        await fixture.PublishEndpoint.Publish(message);

        var notification = rabbitMq.Consumed<NotifyMovieAddedToLibrary>();

        var observedMovie = await fixture.Movies
            .Find(x => x.ImdbCode == movie.ImdbCode)
            .FirstAsync();
       
        using var scope = new AssertionScope();
        notification.ImdbCode.Should().Be(movie.ImdbCode);
        observedMovie.LocalSource.Should().BeEquivalentTo(new LocalMovieSource
        {
            Source = "plex",
            DateCreated = date
        }, o => o.DatesToNearestSecond()
            .Excluding(x => x.DateScraped));

        observedMovie.Download!.Statuses.Select(x => x.Status).Should()
            .Contain(new List<MovieDownloadStatusCode>
            {
                MovieDownloadStatusCode.Started,
                MovieDownloadStatusCode.Downloaded,
                MovieDownloadStatusCode.Complete
            });
        
        fixture.FileSystem.Should()
            .ContainFile("/movie-library/Back to the Future Part III (1990)/Back to the Future Part III (1990).mp4", "movie")
            .And.ContainFile("/movie-library/Back to the Future Part III (1990)/Back to the Future Part III (1990).srt", "subs")
            .And.ContainEmptyDirectory("/var/downloads");

        fixture.WireMock.Should().HaveCalledMapping(torrentRemoveUuid, "torrent should have been removed");
    }
}