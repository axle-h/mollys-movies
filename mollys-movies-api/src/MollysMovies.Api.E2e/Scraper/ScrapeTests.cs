using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.E2e.Fixtures;
using MollysMovies.ScraperClient;
using MongoDB.Driver;
using Xunit;

namespace MollysMovies.Api.E2e.Scraper;

public class ScrapeTests : MollysMoviesApiTests
{
    public ScrapeTests(MollysMoviesApiFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Starts_scrape()
    {
        using var rabbitMq = await Fixture.RabbitMq("starts-scrape")
            .Consume<StartScrape>()
            .RunAsync();
        
        var observed = await Fixture.ScrapeApi.ScrapeAsync();
        observed.Should().BeEquivalentTo(new {Success = null as bool?});

        var received = rabbitMq.Consumed<StartScrape>();
        received.Should().Be(new StartScrape(observed.Id!));

        var scrape = await Fixture.Scrapes.Find(x => x.Id == observed.Id).FirstAsync();
        scrape.Should().NotBeNull("should create scrape");
    }
}