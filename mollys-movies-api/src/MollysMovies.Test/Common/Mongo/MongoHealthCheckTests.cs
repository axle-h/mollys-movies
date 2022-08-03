using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MollysMovies.Common.Mongo;
using MollysMovies.Test.Fixtures;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace MollysMovies.Test.Common.Mongo;

public class MongoHealthCheckTests : IClassFixture<AutoMockFixtureBuilder<MongoHealthCheck>>
{
    private readonly AutoMockFixture<MongoHealthCheck> _fixture;

    public MongoHealthCheckTests(AutoMockFixtureBuilder<MongoHealthCheck> builder)
    {
        _fixture = builder
            .InjectMock<IMongoDatabase>()
            .InjectMock<IMongoInitService>()
            .Build();
    }

    [Fact]
    public async Task Unhealthy_when_database_down()
    {
        var exception = new Exception("mongo down");
        _fixture.Mock<IMongoDatabase>(mock =>
        {
            mock.Setup(x => x.ListCollectionNamesAsync(null, CancellationToken.None))
                .ThrowsAsync(exception);
        });

        var context = new HealthCheckContext();
        var observed = await _fixture.Subject.CheckHealthAsync(context);

        observed.Should().BeEquivalentTo(HealthCheckResult.Unhealthy(exception: exception));
    }

    [Fact]
    public async Task Healthy_when_not_throws()
    {
        _fixture.Mock<IMongoDatabase>(mock =>
            {
                mock.Setup(x => x.ListCollectionNamesAsync(null, CancellationToken.None))
                    .ReturnsAsync(new Mock<IAsyncCursor<string>>().Object);
            })
            .Mock<IMongoInitService>(mock =>
            {
                mock.Setup(x => x.WaitAsync(CancellationToken.None)).Returns(Task.CompletedTask);
            });

        var context = new HealthCheckContext();
        var observed = await _fixture.Subject.CheckHealthAsync(context);

        observed.Should().BeEquivalentTo(HealthCheckResult.Healthy());
    }
}