using System.Net;
using System.Net.Http.Json;
using FluentAssertions.Execution;
using MakeMovies.Api.Movies;

namespace MakeMovies.Api.Tests.Movies;

public class MovieApiTests(ApiFixture fixture) : ApiTests(fixture)
{
    [Fact]
    public async Task Attempting_to_get_missing_movie()
    {
        var response = await Fixture.CreateClient().GetAsync("/api/v1/movie/abc123");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Getting_movie()
    {
        var response = await Fixture.CreateClient().GetAsync("/api/v1/movie/yts_3926");
        response.EnsureSuccessStatusCode();
        
        var movie = await response.Content.ReadFromJsonAsync<Movie>();

        using var scope = new AssertionScope();

        movie!.Title.Should().Be("Toy Story");
        movie.ImdbCode.Should().Be("tt0114709");
    }
    
    [Fact]
    public async Task Listing_movies()
    {
        var response = await Fixture.CreateClient().GetAsync("/api/v1/movie?page=1&limit=2&orderBy=Year&descending=true");
        response.EnsureSuccessStatusCode();
        
        var page = await response.Content.ReadFromJsonAsync<PaginatedData<Movie>>();
        page.Should().BeEquivalentTo(new {Page = 1, Limit = 2});
        page!.Data[0].Year.Should().BeGreaterThan(page.Data[1].Year);
    }
    
    [Fact]
    public async Task Searching_movies()
    {
        var response = await Fixture.CreateClient().GetAsync("/api/v1/movie?page=1&limit=10&search=inter");
        response.EnsureSuccessStatusCode();
        
        var page = await response.Content.ReadFromJsonAsync<PaginatedData<Movie>>();
        page.Should().BeEquivalentTo(new {Page = 1, Limit = 10});
        page!.Data[0].Title.Should().Be("Interstellar");
    }
}