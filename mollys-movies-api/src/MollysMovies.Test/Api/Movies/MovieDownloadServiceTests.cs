using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.Common.Exceptions;
using MollysMovies.Api.Movies;
using MollysMovies.Common.Movies;
using MollysMovies.FakeData;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Api.Movies;

public class MovieDownloadServiceTests : IClassFixture<AutoMockFixtureBuilder<MovieDownloadService>>
{
    private readonly AutoMockFixture<MovieDownloadService> _fixture;

    public MovieDownloadServiceTests(AutoMockFixtureBuilder<MovieDownloadService> builder)
    {
        _fixture = builder
            .MockSystemClock()
            .InjectMock<IMovieMapper>()
            .InjectMock<IMovieRepository>()
            .Build();
    }

    [Fact]
    public async Task Getting_movie_by_external_id_but_missing()
    {
        _fixture.Mock<IMovieRepository>(mock =>
        {
            mock.Setup(x => x.GetByExternalDownloadIdAsync("100", CancellationToken.None))
                .ReturnsAsync(null as Movie);
        });

        var act = () => _fixture.Subject.GetMovieByDownloadExternalIdAsync("100");
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Getting_movie_by_external_id()
    {
        var movie = Fake.Movie.Generate();
        var dto = FakeDto.MovieDto.Generate();
        _fixture.Mock<IMovieRepository>(mock =>
            {
                mock.Setup(x => x.GetByExternalDownloadIdAsync("100", CancellationToken.None))
                    .ReturnsAsync(movie);
            })
            .Mock<IMovieMapper>(mock =>
            {
                mock.Setup(x => x.ToMovieDto(movie)).Returns(dto);
            });

        var observed = await _fixture.Subject.GetMovieByDownloadExternalIdAsync("100");
        observed.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Setting_status()
    {
        var imdbCode = Fake.Faker.ImdbCode();
        var movie = Fake.Movie.Generate();

        _fixture.Mock<IMovieRepository>(mock =>
        {
            mock.Setup(x => x.AddDownloadStatus(
                    imdbCode,
                    It.Is<MovieDownloadStatus>(s =>
                        s.Status == MovieDownloadStatusCode.Complete
                        && s.DateCreated == Fake.UtcNow),
                    CancellationToken.None))
                .ReturnsAsync(movie);
        });

        await _fixture.Subject.SetStatusAsync(imdbCode, MovieDownloadStatusCode.Complete);

        _fixture.VerifyAll();
    }

    [Fact]
    public async Task Setting_download()
    {
        var request = FakeDto.SetDownloadRequest.Generate();
        var movie = Fake.Movie.Generate();
        MovieDownload? download = null;

        _fixture.Mock<IMovieRepository>(mock =>
        {
            mock.Setup(x => x.ReplaceDownload(request.ImdbCode, It.IsAny<MovieDownload>(), CancellationToken.None))
                .Callback<string, MovieDownload, CancellationToken>((_, d, _) => download = d)
                .ReturnsAsync(movie);
        });

        await _fixture.Subject.SetDownloadAsync(request);

        download.Should().BeEquivalentTo(new MovieDownload
        {
            ExternalId = request.ExternalId,
            Name = request.Name,
            MagnetUri = request.MagnetUri,
            Quality = request.Quality,
            Type = request.Type,
            Source = request.Source,
            Statuses = new List<MovieDownloadStatus>
            {
                new() {Status = MovieDownloadStatusCode.Started, DateCreated = Fake.UtcNow}
            }
        });

        _fixture.VerifyAll();
    }
}