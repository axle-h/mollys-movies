using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MollysMovies.Common.Scraper;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Plex;
using MollysMovies.Scraper.Plex.Models;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Scraper.Plex;

public class PlexScraperTests : IClassFixture<AutoMockFixtureBuilder<PlexScraper>>
{
    private readonly AutoMockFixture<PlexScraper> _fixture;

    public PlexScraperTests(AutoMockFixtureBuilder<PlexScraper> fixture)
    {
        _fixture = fixture
            .InjectMock<IPlexApiClient>()
            .InjectMock<IPlexMapper>()
            .Build();
    }

    [Fact]
    public void It_should_have_metadata()
    {
        using var scope = new AssertionScope();
        _fixture.Subject.Source.Should().Be("plex");
        _fixture.Subject.Type.Should().Be(ScraperType.Local);
    }

    [Fact]
    public async Task Scraping_but_no_libraries()
    {
        HavingLibraries();

        var session = FakeDto.ScrapeSession.Generate();
        var observed = await _fixture.Subject.ScrapeMoviesAsync(session).ToListAsync();

        observed.Should().BeEmpty();
    }

    [Fact]
    public async Task Scraping_but_no_movies()
    {
        HavingLibraries(FakeDto.PlexLibrary.Generate() with {Key = "1"});
        HavingMeta("1");

        var session = FakeDto.ScrapeSession.Generate();
        var observed = await _fixture.Subject.ScrapeMoviesAsync(session).ToListAsync();

        observed.Should().BeEmpty();
    }

    [Fact]
    public async Task Scraping_but_no_new_movies()
    {
        HavingLibraries(FakeDto.PlexLibrary.Generate() with {Key = "1"});
        HavingMeta("1",
            FakeDto.PlexMovieMetadata.Generate() with {DateCreated = new DateTime(2021, 6, 1, 11, 59, 59)});

        var session = FakeDto.ScrapeSession.Generate() with {ScrapeFrom = new DateTime(2021, 6, 1, 12, 0, 0)};
        var observed = await _fixture.Subject.ScrapeMoviesAsync(session).ToListAsync();

        observed.Should().BeEmpty();
    }

    [Fact]
    public async Task Scraping_but_no_imdb_movies()
    {
        HavingLibraries(FakeDto.PlexLibrary.Generate() with {Key = "1"});
        HavingMeta("1", FakeDto.PlexMovieMetadata.Generate() with {RatingKey = "a"});
        HavingMovie("a", null);

        var session = FakeDto.ScrapeSession.Generate();
        var observed = await _fixture.Subject.ScrapeMoviesAsync(session).ToListAsync();

        observed.Should().BeEmpty();
    }

    [Fact]
    public async Task Successfully_scraping()
    {
        var movies = FakeDto.PlexMovie.Generate(2);
        var requests = FakeDto.CreateLocalMovieRequest.Generate(2);

        HavingLibraries(FakeDto.PlexLibrary.Generate() with {Key = "1"});
        HavingMeta("1",
            FakeDto.PlexMovieMetadata.Generate() with {RatingKey = "a"},
            FakeDto.PlexMovieMetadata.Generate() with {RatingKey = "b"}
        );
        HavingMovie("a", movies[0]);
        HavingMovie("b", movies[1]);

        _fixture.Mock<IPlexMapper>(mock =>
        {
            foreach (var (movie, request) in movies.Zip(requests))
            {
                mock.Setup(x => x.ToCreateLocalMovieRequest(movie)).Returns(request);
            }
        });

        var session = FakeDto.ScrapeSession.Generate();
        var observed = await _fixture.Subject.ScrapeMoviesAsync(session).ToListAsync();

        observed.Should().BeEquivalentTo(requests);
    }

    private void HavingLibraries(params PlexLibrary[] libraries)
    {
        _fixture.Mock<IPlexApiClient>(mock =>
            mock.Setup(x => x.GetMovieLibrariesAsync(CancellationToken.None)).ReturnsAsync(libraries));
    }

    private void HavingMeta(string libraryKey, params PlexMovieMetadata[] meta)
    {
        _fixture.Mock<IPlexApiClient>(mock =>
            mock.Setup(x => x.GetAllMovieMetadataAsync(libraryKey, CancellationToken.None)).ReturnsAsync(meta));
    }

    private void HavingMovie(string ratingKey, PlexMovie? movie)
    {
        _fixture.Mock<IPlexApiClient>(mock =>
            mock.Setup(x => x.GetMovieAsync(ratingKey, CancellationToken.None)).ReturnsAsync(movie));
    }
}