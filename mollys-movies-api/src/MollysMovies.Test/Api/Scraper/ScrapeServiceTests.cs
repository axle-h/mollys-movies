using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MollysMovies.Api.Scraper;
using MollysMovies.FakeData;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Api.Scraper;

public class ScrapeServiceTests : IClassFixture<AutoMockFixtureBuilder<ScrapeService>>
{
    private readonly AutoMockFixture<ScrapeService> _fixture;

    public ScrapeServiceTests(AutoMockFixtureBuilder<ScrapeService> fixture)
    {
        _fixture = fixture
            .InjectMock<IScrapeMapper>()
            .InjectMock<IScrapeRepository>()
            .MockSystemClock()
            .Build();
    }

    [Fact]
    public async Task Getting_all_scrapes()
    {
        var scrapes = Fake.Scrape.Generate(2);
        var dtos = FakeDto.ScrapeDto.Generate(2);

        _fixture
            .Mock<IScrapeRepository>(mock =>
            {
                mock.Setup(x => x.GetAllAsync(CancellationToken.None))
                    .ReturnsAsync(scrapes);
            })
            .Mock<IScrapeMapper>(mock =>
            {
                foreach (var (scrape, dto) in scrapes.Zip(dtos))
                {
                    mock.Setup(x => x.ToScrapeDto(scrape)).Returns(dto);
                }
            });

        var observed = await _fixture.Subject.GetAllAsync();
        observed.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task Creating_scrape()
    {
        var scrape = Fake.Scrape.Generate();
        var dto = FakeDto.ScrapeDto.Generate();
        _fixture
            .Mock<IScrapeRepository>(mock =>
            {
                mock.Setup(x => x.InsertScrapeAsync(Fake.UtcNow, CancellationToken.None))
                    .ReturnsAsync(scrape);
            })
            .Mock<IScrapeMapper>(mock =>
            {
                mock.Setup(x => x.ToScrapeDto(scrape)).Returns(dto);
            });

        var observed = await _fixture.Subject.CreateScrapeAsync();

        using var scope = new AssertionScope();
        observed.Should().Be(dto);

        _fixture.VerifyAll();
    }
}