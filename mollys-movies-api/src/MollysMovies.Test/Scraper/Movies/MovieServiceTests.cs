using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using MollysMovies.Common.Movies;
using MollysMovies.Common.Scraper;
using MollysMovies.FakeData;
using MollysMovies.Scraper.Models;
using MollysMovies.Scraper.Movies;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Scraper.Movies;

public class MovieServiceTests : IClassFixture<AutoMockFixtureBuilder<MovieService>>
{
    private readonly AutoMockFixture<MovieService> _fixture;

    public MovieServiceTests(AutoMockFixtureBuilder<MovieService> builder)
    {
        _fixture = builder
            .InjectMock<IMovieRepository>()
            .InjectMock<IValidator<CreateMovieRequest>>()
            .MockSystemClock()
            .Build();
    }

    [Fact]
    public async Task Getting_new_remote_scrape_session()
    {
        var latestMovieDate = new DateTime(2021, 12, 2);
        _fixture.Mock<IMovieRepository>(m => m
            .Setup(x => x.GetLatestMovieBySourceAsync("some-source", CancellationToken.None))
            .ReturnsAsync(latestMovieDate));

        var observed = await _fixture.Subject.GetScrapeSessionAsync("some-source", ScraperType.Torrent);

        using var scope = new AssertionScope();
        observed.Source.Should().Be("some-source");
        observed.Type.Should().Be(ScraperType.Torrent);
        observed.ScrapeDate.Should().Be(Fake.UtcNow);
        observed.ScrapeFrom.Should().Be(latestMovieDate);
    }

    [Fact]
    public async Task Getting_new_local_scrape_session()
    {
        var latestMovieDate = new DateTime(2021, 12, 2);
        _fixture.Mock<IMovieRepository>(m => m
            .Setup(x => x.GetLatestLocalMovieBySourceAsync("some-source", CancellationToken.None))
            .ReturnsAsync(latestMovieDate));

        var observed = await _fixture.Subject.GetScrapeSessionAsync("some-source", ScraperType.Local);

        using var scope = new AssertionScope();
        observed.Source.Should().Be("some-source");
        observed.Type.Should().Be(ScraperType.Local);
        observed.ScrapeDate.Should().Be(Fake.UtcNow);
        observed.ScrapeFrom.Should().Be(latestMovieDate);
    }

    [Fact]
    public async Task Creating_movie()
    {
        var session = FakeDto.ScrapeSession.Generate() with {Type = ScraperType.Torrent};
        var torrent = FakeDto.CreateTorrentRequest.Generate();
        var request = FakeDto.CreateMovieRequest.Generate() with
        {
            Torrents = new List<CreateTorrentRequest> {torrent}
        };

        MovieMeta? meta = null;
        IEnumerable<Torrent>? torrents = null;
        _fixture.Mock<IMovieRepository>(m =>
            m.Setup(x => x.UpsertFromRemoteAsync(request.ImdbCode, It.IsAny<MovieMeta>(),
                    It.IsAny<IEnumerable<Torrent>>(), CancellationToken.None))
                .Callback<string, MovieMeta, IEnumerable<Torrent>, CancellationToken>((_, mm, ts, _) =>
                {
                    meta = mm;
                    torrents = ts;
                })
                .ReturnsAsync(Fake.Movie.Generate()));

        HavingValidCreateMovieRequest(request);
        
        await _fixture.Subject.CreateMovieAsync(session, request);

        using var scope = new AssertionScope();
        meta.Should().NotBeNull();
        meta!.Source.Should().Be(session.Source);
        meta.Title.Should().Be(request.Title);
        meta.Language.Should().Be(request.Language);
        meta.Year.Should().Be(request.Year);
        meta.Rating.Should().Be(request.Rating);
        meta.Description.Should().Be(request.Description);
        meta.YouTubeTrailerCode.Should().Be(request.YouTubeTrailerCode);
        meta.RemoteImageUrl.Should().Be(request.SourceCoverImageUrl);
        meta.ImageFilename.Should().BeNull();
        meta.Genres.Should().BeEquivalentTo(request.Genres);
        meta.DateCreated.Should().Be(request.DateCreated);
        meta.DateCreated.Should().Be(request.DateCreated);

        torrents.Should().HaveCount(1).And.ContainEquivalentOf(new Torrent
        {
            Source = session.Source,
            Hash = torrent.Hash,
            Quality = torrent.Quality,
            Type = torrent.Type,
            Url = torrent.Url,
            SizeBytes = torrent.SizeBytes
        });
    }

    [Fact]
    public async Task Creating_local_movie()
    {
        var request = FakeDto.CreateLocalMovieRequest.Generate();
        var session = FakeDto.ScrapeSession.Generate() with {Type = ScraperType.Local};

        MovieMeta? meta = null;
        LocalMovieSource? localMovie = null;
        _fixture.Mock<IMovieRepository>(m =>
            m.Setup(x => x.UpsertFromLocalAsync(request.ImdbCode, It.IsAny<MovieMeta>(),
                    It.IsAny<LocalMovieSource>(), CancellationToken.None))
                .Callback<string, MovieMeta, LocalMovieSource, CancellationToken>((_, mm, s, _) =>
                {
                    meta = mm;
                    localMovie = s;
                })
                .ReturnsAsync(Fake.Movie.Generate()));

        await _fixture.Subject.CreateLocalMovieAsync(session, request);

        using var scope = new AssertionScope();
        localMovie.Should().NotBeNull();
        localMovie!.Source.Should().Be(session.Source);
        localMovie.DateCreated.Should().Be(request.DateCreated);
        localMovie.DateScraped.Should().Be(session.ScrapeDate);

        meta.Should().NotBeNull();
        meta!.Source.Should().Be(session.Source);
        meta.Title.Should().Be(request.Title);
        meta.Year.Should().Be(request.Year);
        meta.RemoteImageUrl.Should().Be(request.ThumbPath);
        meta.DateCreated.Should().Be(request.DateCreated);
        meta.DateScraped.Should().Be(session.ScrapeDate);
    }
    
    [Fact]
    public async Task Attempting_to_create_invalid_movie()
    {
        var request = FakeDto.CreateMovieRequest.Generate();
        var session = FakeDto.ScrapeSession.Generate() with {Type = ScraperType.Torrent};

        HavingValidCreateMovieRequest(request, isValid: false);

        var act = () => _fixture.Subject.CreateMovieAsync(session, request);
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("bad things");
    }
    
    [Fact]
    public async Task Attempting_to_create_movie_with_local_session()
    {
        var request = FakeDto.CreateMovieRequest.Generate();
        var session = FakeDto.ScrapeSession.Generate() with {Type = ScraperType.Local};

        var act = () => _fixture.Subject.CreateMovieAsync(session, request);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("scrape session of type Local cannot create torrent movies");
    }

    [Fact]
    public async Task Attempting_to_create_local_movie_with_remote_session()
    {
        var request = FakeDto.CreateLocalMovieRequest.Generate();
        var session = FakeDto.ScrapeSession.Generate() with {Type = ScraperType.Torrent};

        var act = () => _fixture.Subject.CreateLocalMovieAsync(session, request);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("scrape session of type Torrent cannot create local movies");
    }
    
    private void HavingValidCreateMovieRequest(CreateMovieRequest request, bool isValid = true)
    {
        _fixture.Mock<IValidator<CreateMovieRequest>>(m =>
        {
            var setup = m.Setup(x => x.ValidateAsync(
                It.Is<ValidationContext<CreateMovieRequest>>(r => r.InstanceToValidate == request && r.ThrowOnFailures),
                CancellationToken.None));
            if (isValid)
            {
                setup.ReturnsAsync(new ValidationResult());
            }
            else
            {
                setup.ThrowsAsync(new ValidationException("bad things"));
            }
        });
    }
}