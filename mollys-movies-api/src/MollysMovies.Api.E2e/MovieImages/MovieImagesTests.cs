using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.E2e.Fixtures;
using Xunit;

namespace MollysMovies.Api.E2e.MovieImages;

public class MovieImagesTests : MollysMoviesApiTests
{
    public MovieImagesTests(MollysMoviesApiFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Getting_movie_image()
    {
        var response = await Fixture.CreateClient().GetAsync("/movie-images/tt0076759.jpg");
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.ToString().Should().Be("image/jpeg");
    }

    [Fact]
    public async Task Attempting_to_get_non_image()
    {
        var response = await Fixture.CreateClient().GetAsync("/movie-images/not-an-image.txt");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Attempting_to_get_missing_image()
    {
        var response = await Fixture.CreateClient().GetAsync("/movie-images/some-image.jpg");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}