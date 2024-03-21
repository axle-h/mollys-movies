using System.Reflection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MakeMovies.Api.Tests.Meta.Tmdb;

public static class TmdbWireMock
{
    public const string AccessToken = "some-tmdb-token";
    
    public static WireMockServer GivenTmdbConfiguration(this WireMockServer wireMock)
    {
        var baseUrl = $"{wireMock.Urls[0]}/tmdb-images/";
        wireMock.Given(
                Request.Create()
                    .WithPath("/tmdb/3/configuration")
                    .WithHeader("Authorization", $"Bearer {AccessToken}")
                    .UsingGet()
            )
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($"{{ \"images\": {{ \"secure_base_url\": \"{baseUrl}\" }} }}"));
        return wireMock;
    }
    
    public static WireMockServer GivenTmdbFind(this WireMockServer wireMock)
    {
        wireMock.Given(
                Request.Create()
                    .WithPath("/tmdb/3/find/tt0816692")
                    .WithParam("external_source", "imdb_id")
                    .WithHeader("Authorization", $"Bearer {AccessToken}")
                    .UsingGet()
            )
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{ \"movie_results\": [{ \"poster_path\": \"/interstellar.jpg\" }] }"));
        return wireMock;
    }
    
    public static WireMockServer GivenTmdbImage(this WireMockServer wireMock)
    {
        wireMock.Given(
                Request.Create().WithPath("/tmdb-images/w500/interstellar.jpg").UsingGet()
            )
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBinaryBodyFromResource($"{typeof(TmdbWireMock).Namespace}.tt0816692.jpg", "image/jpeg"));
        return wireMock;
    }
}