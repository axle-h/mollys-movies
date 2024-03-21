using System.Net;
using MakeMovies.Api.Tests.Meta.Tmdb;

namespace MakeMovies.Api.Tests.Meta;

public class MetaApiTest(ApiFixture fixture) : ApiTests(fixture)
{
    [Fact]
    public async Task Getting_image()
    {
        Fixture.WireMock
            .GivenTmdbConfiguration()
            .GivenTmdbFind()
            .GivenTmdbImage();

        Fixture.ClientOptions.AllowAutoRedirect = false;
        var startResponse = await Fixture.CreateClient().GetAsync("/api/v1/movie/yts_1632/image");

        startResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        startResponse.Headers.Location!.Should().Be(new Uri("/movie-images/tt0816692.jpg", UriKind.Relative));
        
        var observedBytes = await File.ReadAllBytesAsync(Path.Join(Fixture.TmpPath, "tt0816692.jpg"));
        var expectedBytes = WireMockExtensions.BinaryResource(typeof(MetaApiTest).Namespace + ".Tmdb.tt0816692.jpg");

        observedBytes.Should().BeEquivalentTo(expectedBytes);
    }
}