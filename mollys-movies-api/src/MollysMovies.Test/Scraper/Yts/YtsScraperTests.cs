using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.Common.Scraper;
using MollysMovies.FakeData;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Models;
using MollysMovies.Scraper.Yts;
using MollysMovies.Scraper.Yts.Models;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Scraper.Yts;

public class YtsScraperTests : IClassFixture<AutoMockFixtureBuilder<YtsScraper>>
{
    private readonly AutoMockFixture<YtsScraper> _fixture;

    public YtsScraperTests(AutoMockFixtureBuilder<YtsScraper> fixture)
    {
        _fixture = fixture
            .InjectMock<IYtsClient>()
            .InjectMock<IYtsMapper>()
            .Services(s => s.Configure<ScraperOptions>(x =>
            {
                x.RemoteScrapeDelay = TimeSpan.Zero;
                x.Yts = new YtsOptions
                {
                    Limit = 2,
                    RetryDelay = TimeSpan.Zero
                };
            }))
            .Build();
    }

    [Fact]
    public void Getting_metadata()
    {
        using var scope = new AssertionScope();
        _fixture.Subject.Source.Should().Be("yts");
        _fixture.Subject.Type.Should().Be(ScraperType.Torrent);
    }

    [Fact]
    public async void Scraping_multiple_pages()
    {
        var requests1 = HavingListMoviesPage(1, 2);
        var requests2 = HavingListMoviesPage(2, 1);

        var session = FakeDto.ScrapeSession.Generate();
        var observed = await _fixture.Subject.ScrapeMoviesAsync(session).ToListAsync();

        observed.Should().BeEquivalentTo(requests1.Concat(requests2));
    }

    [Fact]
    public async void Scraping_up_to_scrape_from_date()
    {
        var oldMovie = FakeDto.YtsMovieSummary.Generate() with {DateUploaded = new DateTime(2021, 2, 1, 11, 0, 0)};
        var newMovie = FakeDto.YtsMovieSummary.Generate() with {DateUploaded = new DateTime(2021, 2, 1, 13, 0, 0)};
        var requests = HavingListMoviesPage(1,
            new List<YtsMovieSummary> {oldMovie, newMovie},
            new List<YtsMovieSummary> {newMovie}
        );
        var session = FakeDto.ScrapeSession.Generate() with {ScrapeFrom = new DateTime(2021, 2, 1, 12, 0, 0)};

        var observed = await _fixture.Subject.ScrapeMoviesAsync(session).ToListAsync();

        observed.Should().BeEquivalentTo(requests);
    }

    [Fact]
    public async void Scraping_image()
    {
        var url = Fake.Faker.Internet.Url();
        var image = FakeDto.YtsImage.Generate();
        _fixture.Mock<IYtsClient>(mock =>
            mock.Setup(x => x.GetImageAsync(url, CancellationToken.None)).ReturnsAsync(image));

        var observed = await _fixture.Subject.ScrapeImageAsync(url);

        observed.Should().Be(new ScrapeImageResult(image.Content, image.ContentType));
    }

    private ICollection<CreateMovieRequest> HavingListMoviesPage(int page, int count)
    {
        var movies = FakeDto.YtsMovieSummary.Generate(count);
        return HavingListMoviesPage(page, movies, movies);
    }

    private ICollection<CreateMovieRequest> HavingListMoviesPage(int page, IList<YtsMovieSummary> movies,
        IList<YtsMovieSummary> filteredMovies)
    {
        var requests = FakeDto.CreateMovieRequest.Generate(filteredMovies.Count);
        var response = FakeDto.YtsListMoviesResponse(movies.ToArray());

        _fixture.Mock<IYtsClient>(mock => mock.Setup(x => x.ListMoviesAsync(
                It.Is<YtsListMoviesRequest>(r =>
                    r.Page == page && r.Limit == 2 && r.OrderBy == "desc" && r.SortBy == "date_added"),
                CancellationToken.None
            )).ReturnsAsync(response))
            .Mock<IYtsMapper>(mock =>
            {
                foreach (var (filteredMovie, request) in filteredMovies.Zip(requests))
                {
                    mock.Setup(x => x.ToCreateMovieRequest(filteredMovie)).Returns(request);
                }
            });

        return requests;
    }
}