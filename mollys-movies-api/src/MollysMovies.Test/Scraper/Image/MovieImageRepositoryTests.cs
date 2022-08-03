using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.FakeData;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Image;
using MollysMovies.Test.Fixtures;
using Xunit;

namespace MollysMovies.Test.Scraper.Image;

public class MovieImageRepositoryTests : IClassFixture<AutoMockFixtureBuilder<MovieImageRepository>>
{
    private readonly AutoMockFixture<MovieImageRepository> _fixture;

    public MovieImageRepositoryTests(AutoMockFixtureBuilder<MovieImageRepository> fixture)
    {
        _fixture = fixture
            .InjectFileSystem(f => f.AddFile("/var/images/tt0816692.png", FakeDto.MockFileData.Generate()))
            .Services(services => services.Configure<ScraperOptions>(o => o.ImagePath = "/var/images"))
            .Build();
    }

    [Theory]
    [InlineData("tt0243736", "image/png", ".png")]
    [InlineData("tt0111161", "image/jpg", ".jpg")]
    [InlineData("tt0133093", "image/jpeg", ".jpg")]
    public async Task Creating_movie_image(string imdbCode, string mime, string extension)
    {
        var content = Fake.Faker.Random.Bytes(8);
        var expected = imdbCode + extension;

        var observed = await _fixture.Subject.CreateMovieImageAsync(imdbCode, content, mime);

        observed.Should().Be(expected);
        _fixture.FileSystem().GetFile($"/var/images/{expected}").Contents
            .Should().BeEquivalentTo(content, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Attempting_to_create_unsupported_image()
    {
        var act = () => _fixture.Subject.CreateMovieImageAsync("tt0243736", new byte[] {10, 20, 30}, "image/gif");
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("unknown image mime type image/gif");
    }

    [Fact]
    public void Attempting_to_get_missing_movie_image()
    {
        var observed = _fixture.Subject.GetMovieImage("tt0110912");
        observed.Should().BeNull();
    }

    [Fact]
    public void Getting_movie_image()
    {
        var observed = _fixture.Subject.GetMovieImage("tt0816692");
        observed.Should().Be("tt0816692.png");
    }
}