using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MollysMovies.Common;
using MollysMovies.Common.Scraper;
using MollysMovies.FakeData;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Models;
using MollysMovies.Scraper.Movies;
using MollysMovies.ScraperClient;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Scraper;

public class ScraperServiceTests : IClassFixture<AutoMockFixtureBuilder<ScraperService>>
{
    private static readonly DateTime UtcNow = Fake.UtcNow.AddHours(1);
    private readonly AutoMockFixture<ScraperService> _fixture;

    public ScraperServiceTests(AutoMockFixtureBuilder<ScraperService> fixture)
    {
        _fixture = fixture
            .InjectMock<ISystemClock>()
            .InjectMock<IScraper, ILocalScraper>()
            .InjectMock<IScraper, ITorrentScraper>()
            .InjectMock<IMovieService>()
            .InjectMock<IScraperClient>()
            .InjectMock<IScrapeRepository>()
            .Build();
    }

    [Fact]
    public async Task Attempting_to_scrape_but_scrape_record_doesnt_exist()
    {
        HavingScrape(null);
        var act = () => _fixture.Subject.ScrapeAsync("100", CancellationToken.None);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("cannot find Scrape with id 100");
    }

    [Fact]
    public async Task Scraping_fails()
    {
        var remoteException = new Exception("remote scraping failed");
        var localException = new Exception("local scraping failed");
        var session = FakeDto.ScrapeSession.Generate();
        var scrape = new Scrape {Id = "100", StartDate = Fake.UtcNow};

        HavingScrape(scrape);
        HavingScrapers(session);

        _fixture
            .Mock<ITorrentScraper>(mock =>
            {
                mock.Setup(x => x.ScrapeMoviesAsync(session, CancellationToken.None))
                    .Throws(remoteException);
            })
            .Mock<ILocalScraper>(mock =>
            {
                mock.Setup(x => x.ScrapeMoviesAsync(session, CancellationToken.None))
                    .Throws(localException);
            })
            .Mock<IScrapeRepository>(mock =>
            {
                mock.Setup(x => x.ReplaceAsync(scrape, CancellationToken.None)).Returns(Task.CompletedTask);
            });

        var observed = await _fixture.Subject.ScrapeAsync("100", CancellationToken.None);

        _fixture.VerifyAll();
        scrape.Should().BeEquivalentTo(
            new Scrape
            {
                Id = "100",
                StartDate = Fake.UtcNow,
                EndDate = UtcNow,
                Success = false,
                MovieCount = 0,
                TorrentCount = 0,
                LocalMovieCount = 0,
                Sources = new List<ScrapeSource>
                {
                    new()
                    {
                        Source = "local-scraper",
                        Type = ScraperType.Local,
                        Success = false,
                        Error = localException.ToString(),
                        StartDate = UtcNow,
                        EndDate = UtcNow,
                        MovieCount = 0,
                        TorrentCount = 0
                    },
                    new()
                    {
                        Source = "remote-scraper",
                        Type = ScraperType.Torrent,
                        Success = false,
                        Error = remoteException.ToString(),
                        StartDate = UtcNow,
                        EndDate = UtcNow,
                        MovieCount = 0,
                        TorrentCount = 0
                    }
                }
            }
        );
        observed.Should().BeEquivalentTo(new NotifyScrapeComplete("100", new List<ScrapeSourceFailure>
        {
            new("local-scraper", "Local", localException.ToString()),
            new("remote-scraper", "Torrent", remoteException.ToString())
        }));
    }
    
    [Fact]
    public async Task Ignores_invalid_movies()
    {
        var session = FakeDto.ScrapeSession.Generate();
        var scrape = new Scrape {Id = "100", StartDate = Fake.UtcNow};

        HavingScrape(scrape);
        HavingScrapers(session);

        var remoteRequest = FakeDto.CreateMovieRequest.Generate();

        _fixture
            .Mock<ITorrentScraper>(mock =>
            {
                mock.Setup(x => x.ScrapeMoviesAsync(session, CancellationToken.None))
                    .Returns(new List<CreateMovieRequest> { remoteRequest }.ToAsyncEnumerable());
            })
            .Mock<ILocalScraper>(mock =>
            {
                mock.Setup(x => x.ScrapeMoviesAsync(session, CancellationToken.None))
                    .Returns(new List<CreateLocalMovieRequest>().ToAsyncEnumerable());
            })
            .Mock<IMovieService>(mock =>
            {
                mock.Setup(x => x.CreateMovieAsync(session, remoteRequest, CancellationToken.None))
                    .ThrowsAsync(new ValidationException("bad things"));
            })
            .Mock<IScrapeRepository>(mock =>
            {
                mock.Setup(x => x.ReplaceAsync(scrape, CancellationToken.None)).Returns(Task.CompletedTask);
            });

        var observed = await _fixture.Subject.ScrapeAsync("100", CancellationToken.None);

        _fixture.VerifyAll();
        scrape.Should().BeEquivalentTo(
            new Scrape
            {
                Id = "100",
                StartDate = Fake.UtcNow,
                EndDate = UtcNow,
                Success = true,
                MovieCount = 0,
                TorrentCount = 0,
                LocalMovieCount = 0,
                Sources = new List<ScrapeSource>
                {
                    new()
                    {
                        Source = "local-scraper",
                        Type = ScraperType.Local,
                        Success = true,
                        StartDate = UtcNow,
                        EndDate = UtcNow,
                        MovieCount = 0,
                        TorrentCount = 0
                    },
                    new()
                    {
                        Source = "remote-scraper",
                        Type = ScraperType.Torrent,
                        Success = true,
                        StartDate = UtcNow,
                        EndDate = UtcNow,
                        MovieCount = 0,
                        TorrentCount = 0
                    }
                }
            }
        );
        observed.Should().BeEquivalentTo(new NotifyScrapeComplete("100", new List<ScrapeSourceFailure>()));
    }

    [Fact]
    public async Task Successfully_scraping()
    {
        var session = FakeDto.ScrapeSession.Generate();
        var scrape = new Scrape {Id = "100", StartDate = Fake.UtcNow};

        HavingScrape(scrape);
        HavingScrapers(session);

        var remoteRequests = FakeDto.CreateMovieRequest.Generate(1);
        var localRequests = FakeDto.CreateLocalMovieRequest.Generate(2);

        _fixture
            .Mock<ITorrentScraper>(mock =>
            {
                mock.Setup(x => x.ScrapeMoviesAsync(session, CancellationToken.None))
                    .Returns(remoteRequests.ToAsyncEnumerable());
            })
            .Mock<ILocalScraper>(mock =>
            {
                mock.Setup(x => x.ScrapeMoviesAsync(session, CancellationToken.None))
                    .Returns(localRequests.ToAsyncEnumerable());
            })
            .Mock<IMovieService>(mock =>
            {
                foreach (var request in localRequests)
                {
                    mock.Setup(x => x.CreateLocalMovieAsync(session, request, CancellationToken.None))
                        .Returns(Task.CompletedTask);
                }

                foreach (var request in remoteRequests)
                {
                    mock.Setup(x => x.CreateMovieAsync(session, request, CancellationToken.None))
                        .Returns(Task.CompletedTask);
                }
            })
            .Mock<IScraperClient>(mock =>
            {
                foreach (var request in remoteRequests)
                {
                    mock.Setup(x => x.ScrapeMovieImageAsync(request.ImdbCode, "remote-scraper", request.SourceCoverImageUrl, CancellationToken.None))
                        .Returns(Task.CompletedTask);
                }
            })
            .Mock<IScrapeRepository>(mock =>
            {
                mock.Setup(x => x.ReplaceAsync(scrape, CancellationToken.None)).Returns(Task.CompletedTask);
            });

        var observed = await _fixture.Subject.ScrapeAsync("100", CancellationToken.None);

        _fixture.VerifyAll();
        scrape.Should().BeEquivalentTo(
            new Scrape
            {
                Id = "100",
                StartDate = Fake.UtcNow,
                EndDate = UtcNow,
                Success = true,
                MovieCount = 1,
                TorrentCount = 2,
                LocalMovieCount = 2,
                Sources = new List<ScrapeSource>
                {
                    new()
                    {
                        Source = "local-scraper",
                        Type = ScraperType.Local,
                        Success = true,
                        StartDate = UtcNow,
                        EndDate = UtcNow,
                        MovieCount = 2,
                        TorrentCount = 0
                    },
                    new()
                    {
                        Source = "remote-scraper",
                        Type = ScraperType.Torrent,
                        Success = true,
                        StartDate = UtcNow,
                        EndDate = UtcNow,
                        MovieCount = 1,
                        TorrentCount = 2
                    }
                }
            }
        );
        observed.Should().BeEquivalentTo(new NotifyScrapeComplete("100", new List<ScrapeSourceFailure>()));
    }

    [Fact]
    public async Task Updating_local_movie_libraries()
    {
        _fixture.Mock<ILocalScraper>(mock =>
        {
            mock.Setup(x => x.UpdateMovieLibrariesAsync(CancellationToken.None)).Returns(Task.CompletedTask);
        });
        await _fixture.Subject.UpdateLocalMovieLibrariesAsync(CancellationToken.None);
        _fixture.VerifyAll();
    }

    [Fact]
    public async Task Successfully_scraping_for_local_movie()
    {
        var movie = Fake.Movie.Generate();
        var session = FakeDto.ScrapeSession.Generate();

        HavingLocalScraperOnly(session, movie.ImdbCode);

        var observed = await _fixture.Subject.ScrapeForLocalMovieAsync(movie.ImdbCode, CancellationToken.None);

        observed.Should().BeTrue();
        _fixture.VerifyAll();
    }

    [Fact]
    public async Task Unsuccessfully_scraping_for_local_movie()
    {
        var movie = Fake.Movie.Generate();
        var session = FakeDto.ScrapeSession.Generate();

        HavingLocalScraperOnly(session, "abc123");

        var observed = await _fixture.Subject.ScrapeForLocalMovieAsync(movie.ImdbCode, CancellationToken.None);

        observed.Should().BeFalse();
        _fixture.VerifyAll();
    }

    private void HavingLocalScraperOnly(ScrapeSession session, string imdbCode)
    {
        var createRequest = FakeDto.CreateLocalMovieRequest.Generate() with {ImdbCode = imdbCode};
        _fixture
            .Mock<ILocalScraper>(mock =>
            {
                mock.Setup(x => x.Source).Returns("local-scraper");
                mock.Setup(x => x.Type).Returns(ScraperType.Local);
                mock.Setup(x => x.ScrapeMoviesAsync(session, CancellationToken.None))
                    .Returns(new List<CreateLocalMovieRequest> {createRequest}.ToAsyncEnumerable());
            })
            .Mock<IMovieService>(mock =>
            {
                mock.Setup(x =>
                        x.GetScrapeSessionAsync("local-scraper", ScraperType.Local, CancellationToken.None))
                    .ReturnsAsync(session);
                mock.Setup(x => x.CreateLocalMovieAsync(session, createRequest, CancellationToken.None))
                    .Returns(Task.CompletedTask);
            });
    }

    private void HavingScrape(Scrape? scrape)
    {
        _fixture.Mock<IScrapeRepository>(m => m.Setup(x =>
            x.GetByIdAsync("100", CancellationToken.None)).ReturnsAsync(scrape));
    }

    private void HavingScrapers(ScrapeSession session)
    {
        _fixture
            .MockSystemClock(UtcNow)
            .Mock<ITorrentScraper>(mock =>
            {
                mock.Setup(x => x.Source).Returns("remote-scraper");
                mock.Setup(x => x.Type).Returns(ScraperType.Torrent);
            })
            .Mock<ILocalScraper>(mock =>
            {
                mock.Setup(x => x.Source).Returns("local-scraper");
                mock.Setup(x => x.Type).Returns(ScraperType.Local);
            })
            .Mock<IMovieService>(mock =>
            {
                mock.Setup(x =>
                        x.GetScrapeSessionAsync(It.IsIn("remote-scraper", "local-scraper"), It.IsAny<ScraperType>(), CancellationToken.None))
                    .ReturnsAsync(session);
            });
    }
}