using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MollysMovies.Api.Transmission;
using MollysMovies.Common.TransmissionRpc;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Api.Transmission;

public class TransmissionHealthCheckTests : IClassFixture<AutoMockFixtureBuilder<TransmissionHealthCheck>>
{
    private readonly AutoMockFixture<TransmissionHealthCheck> _fixture;

    public TransmissionHealthCheckTests(AutoMockFixtureBuilder<TransmissionHealthCheck> builder)
    {
        _fixture = builder
            .InjectMock<ITransmissionRpcClient>()
            .Build();
    }

    [Fact]
    public async Task Unhealthy_when_throws()
    {
        var exception = new Exception("transmission down");
        _fixture.Mock<ITransmissionRpcClient>(mock =>
        {
            mock.Setup(x => x.GetAllTorrentsAsync(CancellationToken.None))
                .ThrowsAsync(exception);
        });

        var context = new HealthCheckContext();
        var observed = await _fixture.Subject.CheckHealthAsync(context, CancellationToken.None);

        observed.Should().BeEquivalentTo(HealthCheckResult.Unhealthy(exception: exception));
    }

    [Fact]
    public async Task Healthy_when_not_throws()
    {
        _fixture.Mock<ITransmissionRpcClient>(mock =>
        {
            mock.Setup(x => x.GetAllTorrentsAsync(CancellationToken.None))
                .ReturnsAsync(new List<TorrentInfo>());
        });

        var context = new HealthCheckContext();
        var observed = await _fixture.Subject.CheckHealthAsync(context, CancellationToken.None);
        observed.Should().BeEquivalentTo(HealthCheckResult.Healthy());
    }
}