using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.E2e.Fixtures;
using MollysMovies.Common.Mongo;
using Xunit;

namespace MollysMovies.Api.E2e.Scraper;

public class GetAllScrapesTests : MollysMoviesApiTests
{
    public GetAllScrapesTests(MollysMoviesApiFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Getting_all_scrapes()
    {
        var existingScrape = TestSeedData.Scrapes.First();
        var observed = await Fixture.ScrapeApi.GetAllScrapesAsync();
        observed.Should().NotBeEmpty()
            .And.ContainEquivalentOf(existingScrape, o => o
                .Excluding(x => x.Sources)
                .DatesToNearestSecond());
    }
}