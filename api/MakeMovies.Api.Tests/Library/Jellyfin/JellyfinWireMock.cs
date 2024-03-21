using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MakeMovies.Api.Tests.Library.Jellyfin;

public static class JellyfinWireMock
{
    public const string ApiKey = "some-jellyfin-api-key";
    
    public static WireMockServer GivenJellyfinUsers(this WireMockServer wireMock)
    {
        wireMock.Given(
                Request.Create()
                    .WithPath("/jellyfin/Users")
                    .WithHeader("Authorization", $"Mediabrowser Token=\"{ApiKey}\"")
                    .UsingGet()
            )
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                          [
                            { "Id": "admin-user-id", "Policy": { "IsAdministrator": true } },
                            { "Id": "other-user-id", "Policy": { "IsAdministrator": false } }
                          ]
                          """));
        return wireMock;
    }
    
    public static WireMockServer GivenJellyfinItems(this WireMockServer wireMock)
    {
        wireMock.Given(
                Request.Create()
                    .WithPath("/jellyfin/Users/admin-user-id/Items")
                    .WithParam("hasImdbId", "true")
                    .WithParam("fields", "ProviderIds")
                    .WithParam("includeItemTypes", "Movie")
                    .WithParam("recursive", "true")
                    .WithHeader("Authorization", $"Mediabrowser Token=\"{ApiKey}\"")
                    .UsingGet()
            )
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                          {
                            "Items": [{ "ProviderIds": { "Imdb": "tt0816692" } }]
                          }
                          """));
        return wireMock;
    }
    
    public static Guid GivenJellyfinUpdate(this WireMockServer wireMock)
    {
        var uuid = Guid.NewGuid();
        wireMock.Given(
                Request.Create()
                    .WithPath("/jellyfin/Library/Refresh")
                    .WithHeader("Authorization", $"Mediabrowser Token=\"{ApiKey}\"")
                    .UsingPost()
            )
            .WithGuid(uuid)
            .RespondWith(Response.Create().WithStatusCode(204));
        return uuid;
    }
}