using System.Net;
using MakeMovies.Api.Downloads;
using MakeMovies.Api.Downloads.TransmissionRpc;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;

namespace MakeMovies.Api.Tests.Downloads.TransmissionRpc;

public class TransmissionRpcClientTests
{
    private const string GetAllTorrentsBody = @"{""method"":""torrent-get"",""arguments"":{""fields"":[""eta"",""files"",""id"",""isStalled"",""name"",""percentDone""]}}";

    private readonly MockHttpMessageHandler _mockHttp = new();
    private readonly TransmissionRpcClient _client;
    
    public TransmissionRpcClientTests()
    {
        var options = new OptionsWrapper<DownloadOptions>(new DownloadOptions
        {
            Transmission = new Transmission { Url = new Uri("https://transmission") }
        });
        _client = new TransmissionRpcClient(new HttpClient(_mockHttp), options);
    }

    [Fact]
    public async Task Setting_session_id_after_conflict_response()
    {
        ExpectTransmissionRpc(GetAllTorrentsBody)
            .Respond(HttpStatusCode.Conflict,
                new Dictionary<string, string>
                    {["X-Transmission-Session-Id"] = "oT3wE8FuzHZVCqaYBW7Kf3LP2dc6w5XDSPFRdhZaszuipC8a"},
                "text/html",
                Fake.Resource("Downloads.TransmissionRpc.conflict.html"));

        ExpectTransmissionRpc(GetAllTorrentsBody)
            .WithHeaders("X-Transmission-Session-Id", "oT3wE8FuzHZVCqaYBW7Kf3LP2dc6w5XDSPFRdhZaszuipC8a")
            .RespondWithJsonResource("Downloads.TransmissionRpc.torrent_get_all.json");

        var observed = await _client.GetAllTorrentsAsync();

        observed.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handling_rpc_failure()
    {
        const string response = @"{""result"":""method name not recognized"",""arguments"":{}}";
        ExpectTransmissionRpc(GetAllTorrentsBody)
            .Respond(HttpStatusCode.Accepted, "application/json", response);

        var act = () => _client.GetAllTorrentsAsync();
        await act.Should().ThrowAsync<Exception>().WithMessage($"transmission rpc {GetAllTorrentsBody} failed with result {response}");
    }

    [Fact]
    public async Task Getting_all_torrents()
    {
        ExpectTransmissionRpc(GetAllTorrentsBody)
            .RespondWithJsonResource("Downloads.TransmissionRpc.torrent_get_all.json");

        var observed = await _client.GetAllTorrentsAsync();

        observed.Should().BeEquivalentTo(new List<TorrentInfo>
        {
            new(8, "ubuntu-21.10-desktop-amd64.iso", 0.0102, false, 7672,
                [new TorrentFile("ubuntu-21.10-desktop-amd64.iso")]),
            new(9, "ubuntu-20.04.3-desktop-amd64.iso", 0.0101, true, 7673,
                [new TorrentFile("ubuntu-20.04.3-desktop-amd64.iso")])
        });
    }

    [Fact]
    public async Task Getting_torrent_by_id()
    {
        ExpectTransmissionRpc(
                @"{""method"":""torrent-get"",""arguments"":{""ids"":[8],""fields"":[""eta"",""files"",""id"",""isStalled"",""name"",""percentDone""]}}")
            .RespondWithJsonResource("Downloads.TransmissionRpc.torrent_get.json");

        var observed = await _client.GetTorrentByIdAsync(8);

        observed.Should().BeEquivalentTo(new TorrentInfo(8, "ubuntu-21.10-desktop-amd64.iso", 0.0102, false, 7672,
            new List<TorrentFile> {new("ubuntu-21.10-desktop-amd64.iso")}));
    }

    [Fact]
    public async Task Adding_torrent_by_uri()
    {
        ExpectTransmissionRpc(
                @"{""method"":""torrent-add"",""arguments"":{""filename"":""https://releases.ubuntu.com/21.10/ubuntu-21.10-desktop-amd64.iso.torrent""}}")
            .RespondWithJsonResource("Downloads.TransmissionRpc.torrent_add.json");

        var observed = await _client.AddTorrentAsync("https://releases.ubuntu.com/21.10/ubuntu-21.10-desktop-amd64.iso.torrent");

        observed.Should().BeEquivalentTo(new NewTorrentInfo(8, "ubuntu-21.10-desktop-amd64.iso",
            "f1fcdc1462d36530f526c1d9402eec9100b7ba18"));
    }

    [Fact]
    public async Task Removing_torrent()
    {
        ExpectTransmissionRpc(
                @"{""method"":""torrent-remove"",""arguments"":{""ids"":[8],""delete-local-data"":false}}")
            .Respond(HttpStatusCode.OK, "application/json", @"{""result"":""success"",""arguments"":{}}");

        await _client.RemoveTorrentAsync(8);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    private MockedRequest ExpectTransmissionRpc(string content) => _mockHttp
        .Expect(HttpMethod.Post, "https://transmission/rpc")
        .WithHeaders("Accept", "application/json")
        .WithHeaders("Content-Type", "application/json-rpc")
        .WithContent(content);
}

public static class MockHttpMessageHandlerExtensions
{
    public static MockedRequest RespondWithJsonResource(this MockedRequest source, string resourceName) =>
        source.Respond(HttpStatusCode.OK, "application/json", Fake.Resource(resourceName));
}