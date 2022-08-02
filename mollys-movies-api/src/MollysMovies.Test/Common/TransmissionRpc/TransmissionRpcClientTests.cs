using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Common.TransmissionRpc;
using MollysMovies.FakeData;
using MollysMovies.Test.Fixtures;
using RichardSzalay.MockHttp;
using Xunit;

namespace MollysMovies.Test.Common.TransmissionRpc;

public class TransmissionRpcClientTests : IClassFixture<ApiClientFixture<TransmissionRpcClient>>, IDisposable
{
    private const string GetAllTorrentsBody = @"{""method"":""torrent-get"",""arguments"":{""fields"":[""downloadDir"",""eta"",""files"",""id"",""isStalled"",""name"",""percentDone""]}}";
    private readonly ApiClientFixture<TransmissionRpcClient> _fixture;

    public TransmissionRpcClientTests(ApiClientFixture<TransmissionRpcClient> fixture)
    {
        _fixture = fixture.Configure("https://transmission",
            httpMessageHandlerFactory: mock => new TransmissionRpcClient.TransmissionRpcHandler(mock));
    }

    public void Dispose()
    {
        _fixture.MockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Setting_session_id_after_conflict_response()
    {
        ExpectTransmissionRpc(GetAllTorrentsBody)
            .Respond(HttpStatusCode.Conflict,
                new Dictionary<string, string>
                    {["X-Transmission-Session-Id"] = "oT3wE8FuzHZVCqaYBW7Kf3LP2dc6w5XDSPFRdhZaszuipC8a"},
                "text/html",
                Fake.Resource("Transmission.conflict.html"));

        ExpectTransmissionRpc(GetAllTorrentsBody)
            .WithHeaders("X-Transmission-Session-Id", "oT3wE8FuzHZVCqaYBW7Kf3LP2dc6w5XDSPFRdhZaszuipC8a")
            .RespondWithJsonResource("Transmission.torrent_get_all.json");

        var observed = await _fixture.Subject.GetAllTorrentsAsync();

        observed.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handling_rpc_failure()
    {
        const string response = @"{""result"":""method name not recognized"",""arguments"":{}}";
        ExpectTransmissionRpc(GetAllTorrentsBody)
            .Respond(HttpStatusCode.Accepted, "application/json", response);

        var act = () => _fixture.Subject.GetAllTorrentsAsync();
        await act.Should().ThrowAsync<Exception>().WithMessage($"transmission rpc {GetAllTorrentsBody} failed with result {response}");
    }

    [Fact]
    public async Task Getting_all_torrents()
    {
        ExpectTransmissionRpc(GetAllTorrentsBody)
            .RespondWithJsonResource("Transmission.torrent_get_all.json");

        var observed = await _fixture.Subject.GetAllTorrentsAsync();

        observed.Should().BeEquivalentTo(new List<TorrentInfo>
        {
            new(8, "ubuntu-21.10-desktop-amd64.iso", 0.0102, false, 7672, "/mnt/storage/downloads",
                new List<TorrentFile> {new("ubuntu-21.10-desktop-amd64.iso", 3116482560, 31916032)}),
            new(9, "ubuntu-20.04.3-desktop-amd64.iso", 0.0101, true, 7673, "/mnt/storage/downloads",
                new List<TorrentFile> {new("ubuntu-20.04.3-desktop-amd64.iso", 3116482550, 31916030)})
        });
    }

    [Fact]
    public async Task Getting_torrent_by_id()
    {
        ExpectTransmissionRpc(
                @"{""method"":""torrent-get"",""arguments"":{""ids"":[8],""fields"":[""downloadDir"",""eta"",""files"",""id"",""isStalled"",""name"",""percentDone""]}}")
            .RespondWithJsonResource("Transmission.torrent_get.json");

        var observed = await _fixture.Subject.GetTorrentByIdAsync(8);

        observed.Should().BeEquivalentTo(new TorrentInfo(8, "ubuntu-21.10-desktop-amd64.iso", 0.0102, false, 7672,
            "/mnt/storage/downloads",
            new List<TorrentFile> {new("ubuntu-21.10-desktop-amd64.iso", 3116482560, 31916032)}));
    }

    [Fact]
    public async Task Adding_torrent_by_uri()
    {
        ExpectTransmissionRpc(
                @"{""method"":""torrent-add"",""arguments"":{""filename"":""https://releases.ubuntu.com/21.10/ubuntu-21.10-desktop-amd64.iso.torrent""}}")
            .RespondWithJsonResource("Transmission.torrent_add.json");

        var observed = await _fixture.Subject.AddTorrentAsync("https://releases.ubuntu.com/21.10/ubuntu-21.10-desktop-amd64.iso.torrent");

        observed.Should().BeEquivalentTo(new NewTorrentInfo(8, "ubuntu-21.10-desktop-amd64.iso",
            "f1fcdc1462d36530f526c1d9402eec9100b7ba18"));
    }

    [Fact]
    public async Task Removing_torrent()
    {
        ExpectTransmissionRpc(
                @"{""method"":""torrent-remove"",""arguments"":{""ids"":[8],""delete-local-data"":false}}")
            .Respond(HttpStatusCode.OK, "application/json", @"{""result"":""success"",""arguments"":{}}");

        await _fixture.Subject.RemoveTorrentAsync(8);
    }

    private MockedRequest ExpectTransmissionRpc(string content) => _fixture.MockHttp
        .Expect(HttpMethod.Post, "https://transmission/rpc")
        .WithHeaders("Accept", "application/json")
        .WithHeaders("Content-Type", "application/json-rpc")
        .WithContent(content);
}