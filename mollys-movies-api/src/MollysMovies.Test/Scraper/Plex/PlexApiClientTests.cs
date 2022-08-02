using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Plex;
using MollysMovies.Scraper.Plex.Models;
using MollysMovies.Test.Fixtures;
using RichardSzalay.MockHttp;
using Xunit;

namespace MollysMovies.Test.Scraper.Plex;

public class PlexApiClientTests : IClassFixture<ApiClientFixture<PlexApiClient>>
{
    private readonly ApiClientFixture<PlexApiClient> _fixture;

    public PlexApiClientTests(ApiClientFixture<PlexApiClient> fixture)
    {
        _fixture = fixture.Configure("https://plex",
            s => s.Configure<ScraperOptions>(o => o.Plex = new PlexOptions {Token = "some-token"}));
    }

    [Fact]
    public async Task Getting_movie_libraries()
    {
        _fixture.MockHttp
            .When("https://plex/library/sections")
            .WithQueryString("X-Plex-Token", "some-token")
            .RespondWithXmlResource("Plex.sections.xml");

        var observed = await _fixture.Subject.GetMovieLibrariesAsync();

        observed.Should().BeEquivalentTo(new List<PlexLibrary> {new("5", "movie"), new("4", "movie")});
    }

    [Fact]
    public async Task Getting_all_movie_metadata()
    {
        _fixture.MockHttp
            .When("https://plex/library/sections/5/all")
            .WithQueryString("X-Plex-Token", "some-token")
            .RespondWithXmlResource("Plex.movies.xml");

        var observed = await _fixture.Subject.GetAllMovieMetadataAsync("5");

        observed.Should().BeEquivalentTo(new List<PlexMovieMetadata>
        {
            new("11181", DateTime.Parse("2019-01-20T22:30:27Z")),
            new("11516", DateTime.Parse("2019-01-20T22:01:47Z"))
        });
    }

    [Fact]
    public async Task Updating_library()
    {
        _fixture.MockHttp
            .When("https://plex/library/sections/5/refresh")
            .WithQueryString("X-Plex-Token", "some-token")
            .Respond(HttpStatusCode.OK);

        await _fixture.Subject.UpdateLibraryAsync("5");

        _fixture.MockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Getting_imdb_movie()
    {
        _fixture.MockHttp
            .When("https://plex/library/metadata/11516")
            .WithQueryString("X-Plex-Token", "some-token")
            .RespondWithXmlResource("Plex.metadata.xml");

        var observed = await _fixture.Subject.GetMovieAsync("11516");

        observed.Should().Be(new PlexMovie(
            "tt0816692",
            "Interstellar",
            2014,
            DateTime.Parse("2019-01-20T22:01:47Z"),
            "/library/metadata/11516/thumb/1638304380"
        ));
    }

    [Fact]
    public async Task Attempting_to_get_non_imdb_movie()
    {
        _fixture.MockHttp
            .When("https://plex/library/metadata/11165")
            .WithQueryString("X-Plex-Token", "some-token")
            .RespondWithXmlResource("Plex.non_imdb_metadata.xml");

        var observed = await _fixture.Subject.GetMovieAsync("11165");

        observed.Should().BeNull();
    }

    [Fact]
    public async Task Getting_image()
    {
        _fixture.MockHttp
            .When("https://plex/library/metadata/11516/thumb/1638304380")
            .WithQueryString("X-Plex-Token", "some-token")
            .Respond("image/png", new MemoryStream(new byte[] {1, 2, 3}));

        var (content, contentType) = await _fixture.Subject.GetThumbAsync("/library/metadata/11516/thumb/1638304380");
        contentType.Should().Be("image/png");
        content.Should().BeEquivalentTo(new byte[] {1, 2, 3}, o => o.WithStrictOrdering());
    }
}