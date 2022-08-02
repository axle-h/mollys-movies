using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.E2e.Fixtures;
using MollysMovies.Client.Model;
using Xunit;

namespace MollysMovies.Api.E2e.Movies;

public class SearchMoviesTests : MollysMoviesApiTests
{
    public SearchMoviesTests(MollysMoviesApiFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Paginating_movies()
    {
        var page2 = await Fixture.MoviesApi.SearchMoviesAsync(page: 2, limit: 2);
        page2.Should().BeEquivalentTo(new {Page = 2, Limit = 2});
        page2.Count.Should().BeGreaterThan(0);
        page2.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Ordering_movies_by_title()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync(orderBy: MoviesOrderBy.Title);
        observed.Data.Should().BeInAscendingOrder(x => x.Title);
    }

    [Fact]
    public async Task Ordering_movies_by_year()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync(orderBy: MoviesOrderBy.Year);
        observed.Data.Should().BeInAscendingOrder(x => x.Year);
    }

    [Fact]
    public async Task Ordering_movies_by_rating()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync(orderBy: MoviesOrderBy.Rating);
        observed.Data.Should().BeInAscendingOrder(x => x.Rating);
    }

    [Fact]
    public async Task Ordering_movies_by_rating_descending()
    {
        var observed =
            await Fixture.MoviesApi.SearchMoviesAsync(orderBy: MoviesOrderBy.Rating, orderByDescending: true);
        observed.Data.Should().BeInDescendingOrder(x => x.Rating);
    }

    [Fact]
    public async Task Searching_for_movies_by_title()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync("Interstellar");
        observed.Data.Should()
            .NotBeEmpty()
            .And.ContainMovie("Interstellar");
    }

    [Fact]
    public async Task Searching_for_movies_by_quality()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync(quality: "3D");
        observed.Data.Should()
            .NotBeEmpty()
            .And.ContainMovie("Toy Story")
            .And.NotContainMovie("Interstellar");
    }

    [Fact]
    public async Task Searching_for_movies_by_language()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync(language: "es");
        observed.Data.Should()
            .NotBeEmpty()
            .And.ContainMovie("Pan's Labyrinth")
            .And.NotContainMovie("Interstellar");
    }

    [Fact]
    public async Task Searching_for_movies_by_downloaded()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync(downloaded: true);
        observed.Data.Should()
            .NotBeEmpty()
            .And.ContainMovie("Interstellar")
            .And.NotContainMovie("Mary Poppins");
    }

    [Fact]
    public async Task Searching_for_movies_by_not_downloaded()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync(downloaded: false);
        observed.Data.Should()
            .NotBeEmpty()
            .And.ContainMovie("Mary Poppins")
            .And.NotContainMovie("Interstellar");
    }

    [Fact]
    public async Task Searching_for_movies_by_genre()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync(genre: "Comedy");
        observed.Data.Should()
            .NotBeEmpty()
            .And.ContainMovie("Back to the Future")
            .And.NotContainMovie("Interstellar");
    }

    [Fact]
    public async Task Searching_for_movies_by_year()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync(yearFrom: 2014, yearTo: 2015);
        observed.Data.Should()
            .NotBeEmpty()
            .And.ContainMovie("Chappie") // 2015
            .And.ContainMovie("Interstellar") // 2014
            .And.NotContainMovie("Source Code"); // 2011
    }

    [Fact]
    public async Task Searching_for_movies_by_rating()
    {
        var observed = await Fixture.MoviesApi.SearchMoviesAsync(ratingFrom: 6, ratingTo: 7);
        observed.Data.Should()
            .NotBeEmpty()
            .And.ContainMovie("Flight of the Navigator")
            .And.ContainMovie("Chappie")
            .And.NotContainMovie("Interstellar");
    }
}