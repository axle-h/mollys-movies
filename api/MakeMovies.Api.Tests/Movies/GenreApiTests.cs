using System.Net.Http.Json;

namespace MakeMovies.Api.Tests.Movies;

public class GenreApiTests(ApiFixture fixture) : ApiTests(fixture)
{
    [Fact]
    public async Task Getting_all_genres()
    {
        var response = await Fixture.CreateClient().GetAsync("/api/v1/genre");
        response.EnsureSuccessStatusCode();
        
        var genres = await response.Content.ReadFromJsonAsync<List<string>>();
        genres.Should().NotBeEmpty();
    }
}