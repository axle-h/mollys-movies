using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.Common.Exceptions;
using MollysMovies.Api.Transmission;
using MollysMovies.Api.Transmission.Models;
using MollysMovies.Common.TransmissionRpc;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Api.Transmission;

public class TransmissionServiceTests : IClassFixture<AutoMockFixtureBuilder<TransmissionService>>
{
    private readonly AutoMockFixture<TransmissionService> _fixture;

    public TransmissionServiceTests(AutoMockFixtureBuilder<TransmissionService> builder)
    {
        _fixture = builder
            .InjectMock<ITransmissionRpcClient>()
            .Build();
    }

    [Fact]
    public async Task Downloading_torrent()
    {
        var request = FakeDto.DownloadMovieRequest.Generate() with {Name = "Pulp Fiction (1994)"};
        var newTorrent = FakeDto.NewTorrentInfo.Generate();

        HavingTorrents();
        _fixture.Mock<ITransmissionRpcClient>(mock =>
        {
            mock.Setup(x => x.AddTorrentAsync(request.MagnetUri, CancellationToken.None))
                .ReturnsAsync(newTorrent);
        });

        var observed = await _fixture.Subject.DownloadTorrentAsync(request);

        observed.Should().Be(newTorrent.Id.ToString());
        _fixture.VerifyAll();
    }

    [Fact]
    public async Task Downloading_torrent_but_torrent_already_downloading()
    {
        var request = FakeDto.DownloadMovieRequest.Generate() with {Name = "Interstellar (2014)"};

        HavingTorrents();

        var act = () => _fixture.Subject.DownloadTorrentAsync(request);
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Interstellar (2014) is already downloading");
    }

    [Fact]
    public async Task Getting_live_transmission_status()
    {
        var request = FakeDto.GetLiveTransmissionStatusRequest.Generate() with
        {
            Name = "Interstellar (2014)",
            ExternalId = "888"
        };
        HavingTorrent(FakeDto.TorrentInfo.Generate() with
        {
            Id = 888,
            PercentDone = 0.85,
            IsStalled = false,
            Eta = 120
        });

        var observed = await _fixture.Subject.GetLiveTransmissionStatusAsync(request);

        observed.Should().Be(
            new LiveTransmissionStatusDto(
                "Interstellar (2014)",
                false,
                false,
                120,
                0.85)
        );
    }

    [Fact]
    public async Task Getting_complete_transmission_status()
    {
        var request = FakeDto.GetLiveTransmissionStatusRequest.Generate() with
        {
            Name = "Interstellar (2014)",
            ExternalId = "888"
        };
        HavingTorrent(FakeDto.TorrentInfo.Generate() with
        {
            Id = 888,
            PercentDone = 1.0,
            IsStalled = false,
            Eta = 0
        });

        var observed = await _fixture.Subject.GetLiveTransmissionStatusAsync(request);

        observed.Should().Be(
            new LiveTransmissionStatusDto(
                "Interstellar (2014)",
                true,
                false,
                null,
                1.0)
        );
    }

    [Fact]
    public async Task Getting_missing_transmission_status()
    {
        var request = FakeDto.GetLiveTransmissionStatusRequest.Generate() with
        {
            Name = "Interstellar (2014)",
            ExternalId = "888"
        };
        _fixture.Mock<ITransmissionRpcClient>(mock => mock.Setup(x => x
                .GetTorrentByIdAsync(888, CancellationToken.None))
            .ReturnsAsync(null as TorrentInfo));

        var observed = await _fixture.Subject.GetLiveTransmissionStatusAsync(request);

        observed.Should().Be(new LiveTransmissionStatusDto("Interstellar (2014)", true));
    }

    private void HavingTorrent(TorrentInfo torrent)
    {
        _fixture.Mock<ITransmissionRpcClient>(mock => mock.Setup(x => x
                .GetTorrentByIdAsync(torrent.Id, CancellationToken.None))
            .ReturnsAsync(torrent));
    }

    private void HavingTorrents()
    {
        var torrent = FakeDto.TorrentInfo.Generate() with {Name = "Interstellar (2014)"};
        _fixture.Mock<ITransmissionRpcClient>(mock => mock.Setup(x => x
                .GetAllTorrentsAsync(CancellationToken.None))
            .ReturnsAsync(new List<TorrentInfo> {torrent}));
    }
}