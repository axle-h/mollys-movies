using System;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MollysMovies.Api.E2e.Fixtures;

public static class WireMockExtensions
{
    public static WireMockServer GivenTransmissionRpc(this WireMockServer wireMock,
        string method,
        object arguments,
        object response,
        Guid? uuid = null)
    {
        wireMock.Given(
                Request.Create()
                    .WithPath("/transmission/rpc")
                    .WithHeader("Content-Type", "application/json-rpc")
                    .UsingPost()
                    .WithBody(new JsonPartialMatcher(new {method, arguments}))
            )
            .WithGuid(uuid ?? Guid.NewGuid())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new {result = "success", arguments = response})
            );

        return wireMock;
    }
}