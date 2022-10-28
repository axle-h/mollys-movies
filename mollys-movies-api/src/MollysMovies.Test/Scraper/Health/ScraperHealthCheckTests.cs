using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Health;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Scraper.Health;

public class ScraperHealthCheckTests : IClassFixture<AutoMockFixtureBuilder<ScraperHealthCheck>>
{
    private readonly AutoMockFixture<ScraperHealthCheck> _fixture;

    public ScraperHealthCheckTests(AutoMockFixtureBuilder<ScraperHealthCheck> builder)
    {
        _fixture = builder
            .InjectMock<IScraper>()
            .Services(s => s.AddMemoryCache())
            .Build();
        
        _fixture.Provider.GetRequiredService<IMemoryCache>().Remove("health-some-scraper");
    }

    private HealthCheckContext Context => new()
    {
        Registration = new HealthCheckRegistration("some-scraper", _fixture.Subject, null, null)
    };

    [Fact]
    public async Task Unhealthy_when_throws()
    {
        var exception = new Exception("scraper down");
        _fixture.Mock<IScraper>(mock =>
        {
            mock.Setup(x => x.Source).Returns("some-scraper");
            mock.Setup(x => x.HealthCheckAsync(CancellationToken.None)).ThrowsAsync(exception);
        });


        var observed = await _fixture.Subject.CheckHealthAsync(Context, CancellationToken.None);

        observed.Should().BeEquivalentTo(HealthCheckResult.Unhealthy(exception: exception));
    }

    [Fact]
    public async Task Healthy_when_not_throws()
    {
        _fixture.Mock<IScraper>(mock =>
        {
            mock.Setup(x => x.Source).Returns("some-scraper");
            mock.Setup(x => x.HealthCheckAsync(CancellationToken.None)).Returns(Task.CompletedTask);
        });

        var observed = await _fixture.Subject.CheckHealthAsync(Context, CancellationToken.None);

        observed.Should().BeEquivalentTo(HealthCheckResult.Healthy());
    }

    [Fact]
    public async Task Unhealthy_when_scraper_not_exists()
    {
        _fixture.Mock<IScraper>(mock =>
        {
            mock.Setup(x => x.Source).Returns("some-other-scraper");
        });

        var act = () => _fixture.Subject.CheckHealthAsync(Context, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>("cannot find scraper with name some-other-scraper");
    }
}