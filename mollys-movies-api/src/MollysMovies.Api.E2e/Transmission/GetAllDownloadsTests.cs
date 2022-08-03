using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MollysMovies.Api.E2e.Fixtures;
using MollysMovies.Common.Mongo;
using Xunit;

namespace MollysMovies.Api.E2e.Transmission;

public class GetAllDownloadsTests : MollysMoviesApiTests
{
    public GetAllDownloadsTests(MollysMoviesApiFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Getting_all_downloads()
    {
        var (imdbCode, existingDownload) = TestSeedData.Movies
            .Select(x => (x.ImdbCode, x.Download))
            .First(x => x.Download is not null)!;

        var observed = await Fixture.TransmissionApi.GetAllDownloadsAsync();

        using var scope = new AssertionScope();
        observed.Should().BeEquivalentTo(new {Page = 1, Limit = 20});
        observed.Count.Should().BeGreaterThan(0);
        observed.Data.Should().NotBeEmpty()
            .And.HaveCount((int) observed.Count)
            .And.ContainEquivalentOf(existingDownload, o => o
                .Including(x => x.ExternalId)
                .Including(x => x.Name))
            .Which.ImdbCode.Should().Be(imdbCode);
    }
}