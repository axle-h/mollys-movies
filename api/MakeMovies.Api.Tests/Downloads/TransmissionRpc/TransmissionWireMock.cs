using MakeMovies.Api.Downloads.TransmissionRpc;
using MakeMovies.Api.Movies;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MakeMovies.Api.Tests.Downloads.TransmissionRpc;

public static class TransmissionWireMock
{
    public static WireMockServer GivenMissingTransmissionStatus(this WireMockServer wireMock, int id) =>
        wireMock.GivenRpc("torrent-get",
            new {ids = new[] { id }, fields = TransmissionRpcClient.AllFields},
            new
            {
                removed = Array.Empty<string>(),
                torrents = Array.Empty<object>()
            });

    public static readonly string[] Files = ["Some Movie (2024) [YTS]/movie.mp4", "Some Movie (2024) [YTS]/subtitles.srt", "Some Movie (2024) [YTS]/junk.jpg"];

    public static WireMockServer GivenTransmissionStatus(this WireMockServer wireMock, int id, double percentDone, int eta) =>
        wireMock.GivenRpc("torrent-get",
            new {ids = new[] { id }, fields = TransmissionRpcClient.AllFields},
            new
            {
                removed = Array.Empty<string>(),
                torrents = new[] {new
                {
                    id,
                    name = "Some Movie (2024) [YTS]",
                    percentDone,
                    isStalled = false,
                    eta,
                    files = Files.Select(name => new { name }).ToArray()
                } }
            });

    public static Guid GivenHappyTransmissionStateForDownload(this WireMockServer wireMock)
    {
        var torrentAddId = Guid.NewGuid();
        var magnetUri =
            "magnet:?xt=urn:btih:abc123&dn=Some%20Movie%20%282024%29&tr=udp://tracker1:1337/announce&tr=udp://tracker2:1337/announce";
        wireMock
            // given torrent doesnt exist
            .GivenRpc("torrent-get", new { fields = TransmissionRpcClient.AllFields },
                new {removed = Array.Empty<string>(), torrents = Array.Empty<string>()})
            // given torrent added
            .GivenRpc("torrent-add", new {filename = magnetUri}, new Dictionary<string, object>
            {
                ["torrent-added"] = new {hashString = "abc123", id = 123, name = "Some Movie (2024)"}
            }, torrentAddId);
        return torrentAddId;
    }
    
    private static WireMockServer GivenRpc(this WireMockServer wireMock,
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
                    .WithBody(new JsonMatcher(new {method, arguments}))
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