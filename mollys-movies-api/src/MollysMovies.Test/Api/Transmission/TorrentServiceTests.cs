using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.Common.Exceptions;
using MollysMovies.Api.Movies;
using MollysMovies.Api.Movies.Requests;
using MollysMovies.Api.Transmission;
using MollysMovies.Api.Transmission.Models;
using MollysMovies.Common.Movies;
using MollysMovies.Scraper;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;
using IMovieService = MollysMovies.Api.Movies.IMovieService;

namespace MollysMovies.Test.Api.Transmission;

public class TorrentServiceTests : IClassFixture<AutoMockFixtureBuilder<TorrentService>>
{
    private readonly AutoMockFixture<TorrentService> _fixture;

    public TorrentServiceTests(AutoMockFixtureBuilder<TorrentService> builder)
    {
        _fixture = builder
            .MockSystemClock()
            .InjectMock<IMovieDownloadService>()
            .InjectMock<IMovieService>()
            .InjectMock<ITorrentScraper>()
            .InjectMock<ITransmissionService>()
            .InjectMock<IMagnetUriService>()
            .Build();
    }

    [Fact]
    public async Task Downloading_torrent_but_missing_torrent()
    {
        var movie = FakeDto.MovieDto.Generate() with {ImdbCode = "tt0816692"};
        _fixture
            .Mock<IMovieService>(mock => mock.Setup(x => x.GetAsync("tt0816692", CancellationToken.None)).ReturnsAsync(movie));

        var act = () => _fixture.Subject.DownloadMovieTorrentAsync(movie.ImdbCode, "some-quality", "some-type");
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("torrent with quality 'some-quality' and type 'some-type' does not exist on movie with imdb code 'tt0816692'");
    }

    [Fact]
    public async Task Downloading_torrent_but_already_downloaded()
    {
        var movie = FakeDto.MovieDto.Generate() with {ImdbCode = "tt0816692"};
        var torrent = movie.Torrents.First();

        _fixture
            .Mock<IMovieService>(mock => mock.Setup(x => x.GetAsync("tt0816692", CancellationToken.None)).ReturnsAsync(movie));

        var act = () => _fixture.Subject.DownloadMovieTorrentAsync("tt0816692", torrent.Quality, torrent.Type);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("movie with imdb code 'tt0816692' is already downloaded");
    }

    [Fact]
    public async Task Downloading_torrent_but_currently_downloading()
    {
        var movie = FakeDto.MovieDto.Generate() with {ImdbCode = "tt0816692", LocalSource = null};
        var torrent = movie.Torrents.First();
        _fixture
            .Mock<IMovieService>(mock => mock.Setup(x => x.GetAsync("tt0816692", CancellationToken.None)).ReturnsAsync(movie));
        var act = () => _fixture.Subject.DownloadMovieTorrentAsync("tt0816692", torrent.Quality, torrent.Type);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("movie with imdb code 'tt0816692' is already downloading");
    }

    [Fact]
    public async Task Downloading_torrent()
    {
        var movie = FakeDto.MovieDto.Generate() with
        {
            ImdbCode = "tt0816692",
            Title = "Interstellar",
            Year = 2014,
            LocalSource = null,
            Download = null
        };
        var torrent = movie.Torrents.First();

        _fixture
            .Mock<IMovieService>(mock =>
            {
                mock.Setup(x => x.GetAsync("tt0816692", CancellationToken.None)).ReturnsAsync(movie);
            })
            .Mock<ITorrentScraper>(mock =>
            {
                mock.Setup(x => x.Source).Returns(torrent.Source);
            })
            .Mock<IMagnetUriService>(mock =>
            {
                mock.Setup(x => x.BuildMagnetUri("Interstellar (2014)", torrent.Hash))
                    .Returns("udp://tracker1:1337/announce");
            })
            .Mock<ITransmissionService>(mock =>
            {
                mock.Setup(x =>
                        x.DownloadTorrentAsync(
                            new DownloadMovieRequest("Interstellar (2014)", "udp://tracker1:1337/announce"),
                            CancellationToken.None))
                    .ReturnsAsync("100");
            })
            .Mock<IMovieDownloadService>(mock =>
            {
                var request = new SetDownloadRequest("tt0816692", torrent.Source, torrent.Quality, torrent.Type, "100",
                    "Interstellar (2014)", "udp://tracker1:1337/announce");
                mock.Setup(x => x.SetDownloadAsync(request, CancellationToken.None))
                    .Returns(Task.CompletedTask);
            });

        await _fixture.Subject.DownloadMovieTorrentAsync("tt0816692", torrent.Quality, torrent.Type);

        _fixture.GetMock<IMovieDownloadService>().VerifyAll();
    }

    [Fact]
    public async Task Getting_live_torrent_status_but_download_missing()
    {
        var movie = FakeDto.MovieDto.Generate() with {ImdbCode = "tt0816692", Download = null};
        _fixture
            .Mock<IMovieService>(mock =>
            {
                mock.Setup(x => x.GetAsync("tt0816692", CancellationToken.None)).ReturnsAsync(movie);
            });
        var act = () => _fixture.Subject.GetLiveTransmissionStatusAsync("tt0816692");
        await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage("cannot find MovieDownload with keys {\"ImdbCode\":\"tt0816692\"}");
    }

    [Theory]
    [InlineData(MovieDownloadStatusCode.Downloaded)]
    [InlineData(MovieDownloadStatusCode.Complete)]
    public async Task Getting_complete_torrent_status(MovieDownloadStatusCode status)
    {
        var movie = FakeDto.MovieDto.Generate() with
        {
            ImdbCode = "tt0816692",
            Download = FakeDto.MovieDownloadDto.Generate() with {Status = status}
        };
        _fixture
            .Mock<IMovieService>(mock =>
            {
                mock.Setup(x => x.GetAsync("tt0816692", CancellationToken.None)).ReturnsAsync(movie);
            });

        var result = await _fixture.Subject.GetLiveTransmissionStatusAsync("tt0816692");

        result.Should().Be(new LiveTransmissionStatusDto(movie.Download!.Name, true));
    }

    [Fact]
    public async Task Getting_active_torrent_status()
    {
        var dto = FakeDto.LiveTransmissionStatusDto.Generate();

        var movie = FakeDto.MovieDto.Generate() with
        {
            ImdbCode = "tt0816692",
            Download = FakeDto.MovieDownloadDto.Generate() with {Status = MovieDownloadStatusCode.Started}
        };
        _fixture
            .Mock<IMovieService>(mock =>
            {
                mock.Setup(x => x.GetAsync("tt0816692", CancellationToken.None)).ReturnsAsync(movie);
            })
            .Mock<ITransmissionService>(mock =>
            {
                var request = new GetLiveTransmissionStatusRequest(movie.Download!.ExternalId, movie.Download.Name);
                mock.Setup(x => x.GetLiveTransmissionStatusAsync(request, CancellationToken.None)).ReturnsAsync(dto);
            });

        var result = await _fixture.Subject.GetLiveTransmissionStatusAsync("tt0816692");
        result.Should().BeSameAs(dto);
    }
}