using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.Common.Movies;
using MollysMovies.Common.TransmissionRpc;
using MollysMovies.FakeData;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Movies;
using MollysMovies.ScraperClient;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Scraper.Movies;

public class MovieDownloadCompleteServiceTests : IClassFixture<AutoMockFixtureBuilder<MovieDownloadCompleteService>>
{
    private readonly AutoMockFixture<MovieDownloadCompleteService> _fixture;
    
    public MovieDownloadCompleteServiceTests(AutoMockFixtureBuilder<MovieDownloadCompleteService> builder)
    {
        _fixture = builder
            .InjectMock<IMovieRepository>()
            .InjectMock<ITransmissionRpcClient>()
            .InjectMock<IMovieLibraryService>()
            .InjectMock<IMovieService>()
            .InjectMock<IScraperService>()
            .Services(s => s.Configure<ScraperOptions>(o =>
            {
                o.LocalUpdateMovieDelay = TimeSpan.Zero;
            }))
            .Build();
    }
    
    [Theory]
    [InlineData(MovieDownloadStatusCode.Downloaded)]
    [InlineData(MovieDownloadStatusCode.Complete)]
    public async Task Completing_active_but_context_not_active_and_torrent_still_removed(MovieDownloadStatusCode status)
    {
        var movie = Fake.Movie
            .With(m => m.Download = Fake.MovieDownload.Generate($"default,{status}"))
            .Generate();
        _fixture
            .Mock<IMovieRepository>(mock =>
            {
                mock.Setup(x => x.GetMovieByDownloadExternalIdAsync("10", CancellationToken.None))
                    .ReturnsAsync(movie);
            })
            .Mock<ITransmissionRpcClient>(mock =>
            {
                // should still remove torrent
                mock.Setup(x => x.RemoveTorrentAsync(10, CancellationToken.None))
                    .Returns(Task.CompletedTask);
            });

        var act = () => _fixture.Subject.CompleteActiveAsync("10");
        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"movie with download external id '10' has inactive download '{status}'");

        _fixture.VerifyAll();
    }

    [Fact]
    public async Task Completing_active()
    {
        var movie = Fake.Movie
            .With(m => m.Download = Fake.MovieDownload.Generate("default,Started"))
            .Generate();
        var torrentInfo = FakeDto.TorrentInfo.Generate();
        _fixture
            .Mock<IMovieRepository>(mock =>
            {
                mock.Setup(x => x.GetMovieByDownloadExternalIdAsync("10", CancellationToken.None))
                    .ReturnsAsync(movie);  
            })
            .Mock<IMovieService>(mock =>
            {
                mock.Setup(x => x.SetStatusAsync(movie.ImdbCode, MovieDownloadStatusCode.Downloaded, CancellationToken.None))
                    .Returns(Task.CompletedTask);
                mock.Setup(x => x.SetStatusAsync(movie.ImdbCode, MovieDownloadStatusCode.Complete, CancellationToken.None))
                    .Returns(Task.CompletedTask);
            })
            .Mock<ITransmissionRpcClient>(mock =>
            {
                mock.Setup(x => x.GetTorrentByIdAsync(10, CancellationToken.None)).ReturnsAsync(torrentInfo);
                mock.Setup(x => x.RemoveTorrentAsync(10, CancellationToken.None))
                    .Returns(Task.CompletedTask);
            })
            .Mock<IMovieLibraryService>(mock =>
            {
                mock.Setup(x => x.AddMovie(movie.Download!.Name!, torrentInfo));
            })
            .Mock<IScraperService>(mock =>
            {
                mock.Setup(x => x.UpdateLocalMovieLibrariesAsync(CancellationToken.None)).Returns(Task.CompletedTask);
                mock.Setup(x => x.ScrapeForLocalMovieAsync(movie.ImdbCode, CancellationToken.None)).ReturnsAsync(true);
            });

        var observed = await _fixture.Subject.CompleteActiveAsync("10");

        _fixture.VerifyAll();
        observed.Should().Be(new NotifyMovieAddedToLibrary(movie.ImdbCode));
    }
    
    [Fact]
    public async Task Completing_active_still_removes_torrent_when_movie_not_scraped_from_local()
    {
        var movie = Fake.Movie
            .With(m => m.Download = Fake.MovieDownload.Generate("default,Started"))
            .Generate();
        var torrentInfo = FakeDto.TorrentInfo.Generate();
        _fixture
            .Mock<IMovieRepository>(mock =>
            {
                mock.Setup(x => x.GetMovieByDownloadExternalIdAsync("10", CancellationToken.None))
                    .ReturnsAsync(movie);  
            })
            .Mock<IMovieService>(mock =>
            {
                mock.Setup(x => x.SetStatusAsync(movie.ImdbCode, MovieDownloadStatusCode.Downloaded, CancellationToken.None))
                    .Returns(Task.CompletedTask);
            })
            .Mock<ITransmissionRpcClient>(mock =>
            {
                mock.Setup(x => x.GetTorrentByIdAsync(10, CancellationToken.None)).ReturnsAsync(torrentInfo);
                mock.Setup(x => x.RemoveTorrentAsync(10, CancellationToken.None))
                    .Returns(Task.CompletedTask);
            })
            .Mock<IMovieLibraryService>(mock =>
            {
                mock.Setup(x => x.AddMovie(movie.Download!.Name!, torrentInfo));
            })
            .Mock<IScraperService>(mock =>
            {
                mock.Setup(x => x.UpdateLocalMovieLibrariesAsync(CancellationToken.None)).Returns(Task.CompletedTask);
                mock.Setup(x => x.ScrapeForLocalMovieAsync(movie.ImdbCode, CancellationToken.None)).ReturnsAsync(false);
            });

        var observed = await _fixture.Subject.CompleteActiveAsync("10");

        _fixture.VerifyAll();
        observed.Should().Be(new NotifyMovieAddedToLibrary(movie.ImdbCode));
    }
}