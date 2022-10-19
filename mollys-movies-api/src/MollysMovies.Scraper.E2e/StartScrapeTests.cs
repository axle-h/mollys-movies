using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MassTransit;
using MollysMovies.Common.Movies;
using MollysMovies.Common.Scraper;
using MollysMovies.FakeData;
using MollysMovies.FakeData.FileSystem;
using MollysMovies.Scraper.E2e.Fixtures;
using MollysMovies.Scraper.Plex.Models;
using MollysMovies.ScraperClient;
using MongoDB.Driver;
using Xunit;

namespace MollysMovies.Scraper.E2e;

[Collection("Scraper Tests")]
public class StartScrapeTests
{
    [Fact]
    public async Task Fails_when_scrape_does_not_exist()
    {
        await using var fixture = new MollysMoviesScraperFixture("fails-when-scrape-does-not-exist");
        using var rabbitMq = await fixture.RabbitMq()
            .Consume<Fault<StartScrape>>()
            .RunAsync();
        var message = new StartScrape("bacon");

        await fixture.PublishEndpoint.Publish(message);

        var fault = rabbitMq.Consumed<Fault<StartScrape>>();
        fault.Exceptions.Select(x => x.Message).Should().Contain("cannot find Scrape with id bacon");
    }

    [Fact]
    public async Task Records_scrape_failure()
    {
        await using var fixture = new MollysMoviesScraperFixture("records-scrape-failure");
        using var rabbitMq = await fixture.RabbitMq()
            .Consume<NotifyScrapeComplete>()
            .RunAsync();
        var initialScrape = new Scrape {StartDate = DateTime.UtcNow};
        await fixture.Scrapes.InsertOneAsync(initialScrape);
        var message = new StartScrape(initialScrape.Id);

        await fixture.PublishEndpoint.Publish(message);

        var notification = rabbitMq.Consumed<NotifyScrapeComplete>();
        var scrape = await fixture.Scrapes.Find(x => x.Id == initialScrape.Id).FirstAsync();

        using var scope = new AssertionScope();
        notification.Id.Should().Be(initialScrape.Id);
        scrape.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        scrape.Sources.Should()
            .AllSatisfy(s =>
            {
                s.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
                s.Error.Should().NotBeNullOrEmpty();
            });
        scrape.Should().BeEquivalentTo(new Scrape
        {
            Id = initialScrape.Id,
            Success = false,
            StartDate = initialScrape.StartDate,
            Sources = new List<ScrapeSource>
            {
                new()
                {
                    Source = "yts",
                    Type = ScraperType.Torrent,
                    Success = false
                },
                new()
                {
                    Source = "plex",
                    Type = ScraperType.Local,
                    Success = false
                }
            }
        }, o => o
            .DatesToNearestSecond()
            .Excluding(x => x.EndDate)
            .ExcludingPropertiesOf<Scrape, ScrapeSource>(
                x => x.StartDate,
                x => x.EndDate,
                x => x.Error));
    }

    [Fact]
    public async Task Successfully_scrapes()
    {
        await using var fixture = new MollysMoviesScraperFixture("successfully-scrapes");
        using var rabbitMq = await fixture.RabbitMq()
            .Consume<NotifyScrapeComplete>()
            .RunAsync();
        var date = DateTime.Parse("2121-12-01T12:00:00Z");
        fixture.WireMock
            .GivenYtsListMovies()
            .GivenYtsImages()
            .GivenPlexLibraries()
            .GivenPlexMovieLibrary(new PlexMovieMetadata("99", date), new PlexMovieMetadata("100", date))
            .GivenPlexMovieMetadata("99", date, imdbCode: "tt8015984") // dunkirk already in plex
            .GivenPlexMovieMetadata("100", date); // American Psycho

        var initialScrape = new Scrape {StartDate = DateTime.UtcNow};
        await fixture.Scrapes.InsertOneAsync(initialScrape);
        var message = new StartScrape(initialScrape.Id);

        await fixture.PublishEndpoint.Publish(message);

        var notification = rabbitMq.Consumed<NotifyScrapeComplete>();
        var scrape = await fixture.Scrapes.Find(x => x.Id == initialScrape.Id).FirstAsync();

        using var scope = new AssertionScope();
        notification.Id.Should().Be(initialScrape.Id);
        scrape.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        scrape.Sources.Should()
            .AllSatisfy(s =>
            {
                s.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            });
        scrape.Should().BeEquivalentTo(new Scrape
        {
            Id = initialScrape.Id,
            Success = true,
            MovieCount = 4,
            TorrentCount = 8,
            LocalMovieCount = 2,
            StartDate = initialScrape.StartDate,
            Sources = new List<ScrapeSource>
            {
                new()
                {
                    Source = "yts",
                    Type = ScraperType.Torrent,
                    Success = true,
                    MovieCount = 4,
                    TorrentCount = 8
                },
                new()
                {
                    Source = "plex",
                    Type = ScraperType.Local,
                    Success = true,
                    MovieCount = 2
                }
            }
        }, o => o
            .DatesToNearestSecond()
            .Excluding(x => x.EndDate)
            .ExcludingPropertiesOf<Scrape, ScrapeSource>(x => x.StartDate, x => x.EndDate));

        // should scrape the images
        fixture.FileSystem.Should()
            .ContainFile("/movie-images/tt15772866.jpg", "christmas_with_the_chosen_the_messengers_2021")
            .And.ContainFile("/movie-images/tt8015984.jpg", "battle_of_dunkirk_from_disaster_to_triumph_2018");

        var xmas = await fixture.Movies.Find(x => x.ImdbCode == "tt15772866").FirstOrDefaultAsync();
        xmas.Should().NotBeNull("should create movie with imdb code tt15772866");
        xmas.Should().BeEquivalentTo(new Movie
        {
            ImdbCode = "tt15772866",
            Meta = new MovieMeta
            {
                Source = "yts",
                Title = "Christmas with the Chosen: The Messengers",
                Language = "en",
                Year = 2021,
                Rating = 7.7M,
                Description = "Artists perform new and classic Christmas songs from the set of \"The Chosen.\"",
                YouTubeTrailerCode = "AZ40SqwjIWg",
                ImageFilename = "tt15772866.jpg",
                Genres = new HashSet<string> {"Music"},
                RemoteImageUrl = "/assets/images/movies/christmas_with_the_chosen_the_messengers_2021/large-cover.jpg",
                DateCreated = DateTime.Parse("2121-12-20T12:12:12")
            },
            Torrents = new List<Torrent>
            {
                new()
                {
                    Hash = "1B7E64E9B2CDDEA8B73724F7D81C4C13BDC28892",
                    Quality = "720p",
                    SizeBytes = 1138166333L,
                    Source = "yts",
                    Type = "web",
                    Url = "https://yts.mx/torrent/download/1B7E64E9B2CDDEA8B73724F7D81C4C13BDC28892"
                },
                new()
                {
                    Hash = "2EAF311781F642B86A1511CCAB759D31F2E20E4E",
                    Quality = "1080p",
                    SizeBytes = 2340757176L,
                    Source = "yts",
                    Type = "web",
                    Url = "https://yts.mx/torrent/download/2EAF311781F642B86A1511CCAB759D31F2E20E4E"
                }
            },
            LocalSource = null,
            Download = null
        }, o => o.WithoutStrictOrdering()
            .Excluding(x => x.Meta!.DateScraped));

        var dunkirk = await fixture.Movies.Find(x => x.ImdbCode == "tt8015984").FirstOrDefaultAsync();
        dunkirk.Should().NotBeNull("should create movie with imdb code tt8015984");
        dunkirk.Should().BeEquivalentTo(new Movie
        {
            ImdbCode = "tt8015984",
            Meta = new MovieMeta
            {
                Source = "yts",
                Title = "Battle of Dunkirk: From Disaster to Triumph",
                Language = "en",
                Year = 2018,
                Rating = 8.7M,
                Description = "The events that unfolded at Dunkirk remain one of the greatest stories in human history. In a race against time, over eight hundred defenseless private boats crossed the English Channel to rescue the stranded soldiers from the inferno at the beaches of Dunkirk. Examine the extraordinary personal bravery of individual veterans - from moments of fear and chaos, to their uplifting stories of heroism and sacrifice in a battle that changed the course of WWII.",
                YouTubeTrailerCode = null,
                ImageFilename = "tt8015984.jpg",
                Genres = new HashSet<string> {"Documentary"},
                RemoteImageUrl = "/assets/images/movies/battle_of_dunkirk_from_disaster_to_triumph_2018/large-cover.jpg",
                DateCreated = DateTime.Parse("2121-12-20T13:43:14")
            },
            Torrents = new List<Torrent>
            {
                new()
                {
                    Hash = "36C95B2027F813A4F9585A747B3EF83A0A010A01",
                    Quality = "720p",
                    SizeBytes = 704328499L,
                    Source = "yts",
                    Type = "web",
                    Url = "https://yts.mx/torrent/download/36C95B2027F813A4F9585A747B3EF83A0A010A01"
                },
                new()
                {
                    Hash = "E5EBAECEC177D427B5C99E81B608A30BCA2B996C",
                    Quality = "1080p",
                    SizeBytes = 1309965025L,
                    Source = "yts",
                    Type = "web",
                    Url = "https://yts.mx/torrent/download/E5EBAECEC177D427B5C99E81B608A30BCA2B996C"
                }
            },
            LocalSource = new LocalMovieSource
            {
                DateCreated = DateTime.Parse("2121-12-01T12:00:00"),
                Source = "plex"
            },
            Download = null
        }, o => o.WithoutStrictOrdering()
            .Excluding(x => x.Meta!.DateScraped)
            .Excluding(x => x.LocalSource!.DateScraped));

        var psycho = await fixture.Movies.Find(x => x.ImdbCode == "tt0144084").FirstOrDefaultAsync();
        psycho.Should().NotBeNull("should create movie with imdb code tt0144084");
        psycho.Should().BeEquivalentTo(new Movie
        {
            ImdbCode = "tt0144084",
            Meta = new MovieMeta
            {
                DateCreated = DateTime.Parse("2121-12-01T12:00:00"),
                Genres = new HashSet<string>(),
                RemoteImageUrl = "/library/metadata/100/thumb/4794033600",
                Source = "plex",
                Title = "American Psycho",
                Year = 2000
            },
            LocalSource = new LocalMovieSource
            {
                DateCreated = DateTime.Parse("2121-12-01T12:00:00"),
                Source = "plex"
            }
        }, o => o.WithoutStrictOrdering()
            .Excluding(x => x.Meta!.DateScraped)
            .Excluding(x => x.LocalSource!.DateScraped));

        fixture.FileSystem.Should().ContainFile("/var/downloads/yts_list_movies_1.json", Fake.Resource("Yts.list_movies.json"));
        fixture.FileSystem.Should().ContainFile("/var/downloads/yts_list_movies_2.json", Fake.Resource("Yts.list_movies_2.json"));
        fixture.FileSystem.Should().ContainFile("/var/downloads/yts_list_movies_3.json", Fake.Resource("Yts.list_movies_empty.json"));
    }
}