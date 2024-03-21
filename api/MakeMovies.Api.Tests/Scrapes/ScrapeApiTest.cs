using System.Net.Http.Json;
using FluentAssertions.Execution;
using MakeMovies.Api.Scrapes;
using MakeMovies.Api.Tests.Library.Jellyfin;
using MakeMovies.Api.Tests.Scrapes.Yts;

namespace MakeMovies.Api.Tests.Scrapes;

public class ScrapeApiTest(ApiFixture fixture) : ApiTests(fixture)
{
    [Fact(Timeout = 30000)]
    public async Task Successfully_scrapes()
    {
        Fixture.WireMock
            .GivenYtsListMovies()
            .GivenJellyfinUsers()
            .GivenJellyfinItems();
        
        var startResponse = await Fixture.CreateClient().PostAsync("/api/v1/scrape", null);
        startResponse.EnsureSuccessStatusCode();
        var scrape = await startResponse.Content.ReadFromJsonAsync<Scrape>();

        using (var _ = new AssertionScope())
        {
            scrape!.Id.Should().NotBeNullOrEmpty();
            scrape.StartDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            scrape.EndDate.Should().BeNull();
            scrape.Success.Should().BeNull();
            scrape.MovieCount.Should().Be(0);
            scrape.TorrentCount.Should().Be(0);
            scrape.Error.Should().BeNull();
        }

        var scrapeId = scrape.Id;
        while (true)
        {
            var response = await Fixture.CreateClient().GetAsync("/api/v1/scrape");
            response.EnsureSuccessStatusCode();
            var scrapes = await response.Content.ReadFromJsonAsync<PaginatedData<Scrape>>();
            scrape = scrapes!.Data.FirstOrDefault(s => s.Id == scrapeId)
                ?? throw new Exception("current scrape does not exist");

            if (scrape.EndDate is null)
            {
                await Task.Delay(1000);
                continue;
            }

            using var _ = new AssertionScope();
            scrape.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            scrape.Success.Should().BeTrue();
            scrape.MovieCount.Should().Be(3);
            scrape.TorrentCount.Should().Be(10);
            scrape.Error.Should().BeNull();
            break;
        }
    }
}