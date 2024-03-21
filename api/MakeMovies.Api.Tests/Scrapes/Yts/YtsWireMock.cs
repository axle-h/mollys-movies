using System.Reflection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MakeMovies.Api.Tests.Scrapes.Yts;

public static class YtsWireMock
{
    public static WireMockServer GivenYtsListMovies(this WireMockServer wireMock) =>
        wireMock
            .GivenYtsListMoviesPage(1, "list_movies.json")
            .GivenYtsListMoviesPage(2, "list_movies_2.json")
            .GivenYtsListMoviesPage(3, "list_movies_empty.json");

    private static WireMockServer GivenYtsListMoviesPage(this WireMockServer wireMock, int page, string resource)
    {
        wireMock.Given(
                Request.Create()
                    .WithPath("/yts/api/v2/list_movies.json")
                    .WithParam("page", page.ToString())
                    .WithParam("limit", "2")
                    .WithParam("order_by", "asc")
                    .WithParam("sort_by", "date_added")
                    .UsingGet()
            )
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithJsonBodyFromResource($"{typeof(YtsWireMock).Namespace}.{resource}"));

        return wireMock;
    }
}