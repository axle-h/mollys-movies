using FluentAssertions;
using MollysMovies.Api.Scraper;
using MollysMovies.FakeData;
using Xunit;

namespace MollysMovies.Test.Api.Scraper;

public class ScrapeMapperTests
{
    private readonly ScrapeMapper _subject = new();

    [Fact]
    public void Mapping_Scrape_to_ScrapeDto()
    {
        var source = Fake.Scrape.Generate();
        var observed = _subject.ToScrapeDto(source);
        observed.Should().BeEquivalentTo(source, o => o.Excluding(x => x.Sources));
        observed.Sources.Should().HaveSameCount(source.Sources);
    }

    [Fact]
    public void Mapping_ScrapeSource_to_ScrapeSourceDto()
    {
        var source = Fake.ScrapeSource.Generate();
        var observed = _subject.ToScrapeSourceDto(source);
        observed.Should().BeEquivalentTo(source);
    }
}