using System.Threading;
using System.Threading.Tasks;
using MollysMovies.FakeData;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Image;
using MollysMovies.Scraper.Movies;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Scraper.Image;

public class ImageScraperServiceTests : IClassFixture<AutoMockFixtureBuilder<ImageScraperService>>
{
    private readonly AutoMockFixture<ImageScraperService> _fixture;

    public ImageScraperServiceTests(AutoMockFixtureBuilder<ImageScraperService> fixture)
    {
        _fixture = fixture
            .InjectMock<IMovieImageRepository>()
            .InjectMock<ITorrentScraper>()
            .InjectMock<IMovieRepository>()
            .Build();
    }

    [Fact]
    public async Task Scraping_image_already_exists()
    {
        var request = FakeDto.ScrapeMovieImage.Generate();
        var localPath = Fake.Faker.System.FilePath();
        _fixture.Mock<IMovieImageRepository>(mock =>
            {
                mock.Setup(x => x.GetMovieImage(request.ImdbCode)).Returns(localPath);
            })
            .Mock<IMovieRepository>(mock =>
            {
                mock.Setup(x => x.UpdateMovieImageAsync(request.ImdbCode, localPath, CancellationToken.None))
                    .Returns(Task.CompletedTask);
            });

        await _fixture.Subject.ScrapeImageAsync(request);

        _fixture.VerifyAll();
    }

    [Fact]
    public async Task Successfully_scraping_image()
    {
        var request = FakeDto.ScrapeMovieImage.Generate();
        var image = FakeDto.ScrapeImageResult.Generate();
        var localPath = Fake.Faker.System.FilePath();
        _fixture.Mock<IMovieImageRepository>(m =>
            {
                m.Setup(x => x.GetMovieImage(request.ImdbCode)).Returns(null as string);
                m.Setup(x => x.CreateMovieImageAsync(request.ImdbCode, image.Content, image.ContentType, CancellationToken.None))
                    .ReturnsAsync(localPath);
            })
            .Mock<ITorrentScraper>(mock =>
            {
                mock.Setup(x => x.ScrapeImageAsync(request.Url, CancellationToken.None)).ReturnsAsync(image);
                mock.Setup(x => x.Source).Returns(request.Source);
            })
            .Mock<IMovieRepository>(mock =>
            {
                mock.Setup(x => x.UpdateMovieImageAsync(request.ImdbCode, localPath, CancellationToken.None))
                    .Returns(Task.CompletedTask);
            });

        await _fixture.Subject.ScrapeImageAsync(request);
        _fixture.VerifyAll();
    }
}