using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Yts;
using MollysMovies.Scraper.Yts.Models;
using MollysMovies.Test.Fixtures;
using RichardSzalay.MockHttp;
using Xunit;

namespace MollysMovies.Test.Scraper.Yts;

public class YtsClientTests : IClassFixture<ApiClientFixture<YtsClient>>
{
    private readonly ApiClientFixture<YtsClient> _fixture;

    public YtsClientTests(ApiClientFixture<YtsClient> fixture)
    {
        _fixture = fixture
            .Configure("https://yts",
            s => s
                .AddSingleton<IFileSystem>(new MockFileSystem())
                .Configure<ScraperOptions>(o => o.Yts = new YtsOptions {RetryDelay = TimeSpan.Zero}));
    }

    [Fact]
    public async Task Attempting_to_list_movies_but_api_down()
    {
        _fixture.MockHttp
            .When("https://yts/api/v2/list_movies.json")
            .Respond(HttpStatusCode.InternalServerError);

        var action = () => _fixture.Subject.ListMoviesAsync(new YtsListMoviesRequest());
        await action.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task Listing_movies()
    {
        _fixture.MockHttp
            .When("https://yts/api/v2/list_movies.json")
            .WithQueryString("page", "1")
            .WithQueryString("limit", "2")
            .WithQueryString("order_by", "desc")
            .WithQueryString("sort_by", "date_added")
            .RespondWithJsonResource("Yts.list_movies.json");

        var observed = await _fixture.Subject.ListMoviesAsync(new YtsListMoviesRequest
            {Page = 1, Limit = 2, OrderBy = "desc", SortBy = "date_added"});

        var expectedMovies = new List<YtsMovieSummary>
        {
            new(
                38654,
                new Uri("https://yts.mx/movies/christmas-with-the-chosen-the-messengers-2021"),
                "tt15772866",
                "Christmas with the Chosen: The Messengers",
                "Christmas with the Chosen: The Messengers",
                "Christmas with the Chosen: The Messengers (2021)",
                "christmas-with-the-chosen-the-messengers-2021",
                2021,
                7.7M,
                123,
                new List<string> {"Music"},
                "Artists perform new and classic Christmas songs from the set of \"The Chosen.\"",
                "Artists perform new and classic Christmas songs from the set of \"The Chosen.\"",
                "Artists perform new and classic Christmas songs from the set of \"The Chosen.\"",
                "AZ40SqwjIWg",
                "en",
                "",
                new Uri(
                    "https://yts.mx/assets/images/movies/christmas_with_the_chosen_the_messengers_2021/background.jpg"),
                new Uri(
                    "https://yts.mx/assets/images/movies/christmas_with_the_chosen_the_messengers_2021/background.jpg"),
                new Uri(
                    "https://yts.mx/assets/images/movies/christmas_with_the_chosen_the_messengers_2021/small-cover.jpg"),
                new Uri(
                    "https://yts.mx/assets/images/movies/christmas_with_the_chosen_the_messengers_2021/medium-cover.jpg"),
                new Uri(
                    "https://yts.mx/assets/images/movies/christmas_with_the_chosen_the_messengers_2021/large-cover.jpg"),
                "ok",
                new List<YtsTorrent>
                {
                    new(
                        new Uri("https://yts.mx/torrent/download/1B7E64E9B2CDDEA8B73724F7D81C4C13BDC28892"),
                        "1B7E64E9B2CDDEA8B73724F7D81C4C13BDC28892",
                        "720p",
                        "web",
                        "1.06 GB",
                        1138166333L,
                        DateTime.Parse("2021-12-20T12:12:12Z"),
                        1639998732L
                    ),
                    new(
                        new Uri("https://yts.mx/torrent/download/2EAF311781F642B86A1511CCAB759D31F2E20E4E"),
                        "2EAF311781F642B86A1511CCAB759D31F2E20E4E",
                        "1080p",
                        "web",
                        "2.18 GB",
                        2340757176L,
                        DateTime.Parse("2021-12-20T15:07:57Z"),
                        1640009277L
                    )
                },
                DateTime.Parse("2121-12-20T12:12:12Z"),
                1639998732L
            ),
            new(
                38647,
                new Uri("https://yts.mx/movies/battle-of-dunkirk-from-disaster-to-triumph-2018"),
                "tt8015984",
                "Battle of Dunkirk: From Disaster to Triumph",
                "Battle of Dunkirk: From Disaster to Triumph",
                "Battle of Dunkirk: From Disaster to Triumph (2018)",
                "battle-of-dunkirk-from-disaster-to-triumph-2018",
                2018,
                8.7M,
                65,
                new List<string> {"Documentary"},
                "The events that unfolded at Dunkirk remain one of the greatest stories in human history. In a race against time, over eight hundred defenseless private boats crossed the English Channel to rescue the stranded soldiers from the inferno at the beaches of Dunkirk. Examine the extraordinary personal bravery of individual veterans - from moments of fear and chaos, to their uplifting stories of heroism and sacrifice in a battle that changed the course of WWII.",
                "The events that unfolded at Dunkirk remain one of the greatest stories in human history. In a race against time, over eight hundred defenseless private boats crossed the English Channel to rescue the stranded soldiers from the inferno at the beaches of Dunkirk. Examine the extraordinary personal bravery of individual veterans - from moments of fear and chaos, to their uplifting stories of heroism and sacrifice in a battle that changed the course of WWII.",
                "The events that unfolded at Dunkirk remain one of the greatest stories in human history. In a race against time, over eight hundred defenseless private boats crossed the English Channel to rescue the stranded soldiers from the inferno at the beaches of Dunkirk. Examine the extraordinary personal bravery of individual veterans - from moments of fear and chaos, to their uplifting stories of heroism and sacrifice in a battle that changed the course of WWII.",
                "",
                "en",
                "",
                new Uri(
                    "https://yts.mx/assets/images/movies/battle_of_dunkirk_from_disaster_to_triumph_2018/background.jpg"),
                new Uri(
                    "https://yts.mx/assets/images/movies/battle_of_dunkirk_from_disaster_to_triumph_2018/background.jpg"),
                new Uri(
                    "https://yts.mx/assets/images/movies/battle_of_dunkirk_from_disaster_to_triumph_2018/small-cover.jpg"),
                new Uri(
                    "https://yts.mx/assets/images/movies/battle_of_dunkirk_from_disaster_to_triumph_2018/medium-cover.jpg"),
                new Uri(
                    "https://yts.mx/assets/images/movies/battle_of_dunkirk_from_disaster_to_triumph_2018/large-cover.jpg"),
                "ok",
                new List<YtsTorrent>
                {
                    new(
                        new Uri("https://yts.mx/torrent/download/36C95B2027F813A4F9585A747B3EF83A0A010A01"),
                        "36C95B2027F813A4F9585A747B3EF83A0A010A01",
                        "720p",
                        "web",
                        "671.7 MB",
                        704328499L,
                        DateTime.Parse("2021-12-20T13:43:14Z"),
                        1640004194L
                    ),
                    new(
                        new Uri("https://yts.mx/torrent/download/E5EBAECEC177D427B5C99E81B608A30BCA2B996C"),
                        "E5EBAECEC177D427B5C99E81B608A30BCA2B996C",
                        "1080p",
                        "web",
                        "1.22 GB",
                        1309965025L,
                        DateTime.Parse("2021-12-20T14:35:43Z"),
                        1640007343L
                    )
                },
                DateTime.Parse("2121-12-20T13:43:14Z"),
                1640004194L
            )
        };

        observed.Should().BeEquivalentTo(new YtsListMoviesResponse(37881, 2, 1, expectedMovies));
    }

    [Fact]
    public async Task Getting_image()
    {
        _fixture.MockHttp
            .When("https://yts/some-image.png")
            .Respond("image/png", new MemoryStream(new byte[] {1, 2, 3}));

        var (content, contentType) = await _fixture.Subject.GetImageAsync("/some-image.png");
        contentType.Should().Be("image/png");
        content.Should().BeEquivalentTo(new byte[] {1, 2, 3}, o => o.WithStrictOrdering());
    }
}