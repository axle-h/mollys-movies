using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.Common;
using MollysMovies.Api.Common.Exceptions;
using MollysMovies.Api.Movies;
using MollysMovies.Api.Movies.Models;
using MollysMovies.Api.Movies.Requests;
using MollysMovies.Api.Transmission;
using MollysMovies.Common.Movies;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;
using IMovieService = MollysMovies.Api.Movies.IMovieService;

namespace MollysMovies.Test.Api.Transmission;

public class TransmissionDownloadServiceTests : IClassFixture<AutoMockFixtureBuilder<TransmissionDownloadService>>
{
    private readonly AutoMockFixture<TransmissionDownloadService> _fixture;

    public TransmissionDownloadServiceTests(AutoMockFixtureBuilder<TransmissionDownloadService> builder)
    {
        _fixture = builder
            .InjectMock<IMovieDownloadService>()
            .InjectMock<IMovieService>()
            .Build();
    }

    [Theory]
    [InlineData(MovieDownloadStatusCode.Downloaded)]
    [InlineData(MovieDownloadStatusCode.Complete)]
    public async Task Getting_active_context_by_external_id_but_context_not_active(MovieDownloadStatusCode status)
    {
        var movie = FakeDto.MovieDto.Generate() with {Download = FakeDto.MovieDownloadDto.Generate() with {Status = status}};
        _fixture.Mock<IMovieDownloadService>(mock => mock.Setup(x => x.GetMovieByDownloadExternalIdAsync("10", CancellationToken.None))
            .ReturnsAsync(movie));
        var act = () => _fixture.Subject.GetActiveAsync("10");
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage("cannot find MovieDownload with keys {\"ExternalId\":\"10\"}, context is not active");
    }

    [Fact]
    public async Task Getting_active_context_by_external_id()
    {
        var movie = FakeDto.MovieDto.Generate() with {Download = FakeDto.MovieDownloadDto.Generate() with {Status = MovieDownloadStatusCode.Started}};
        _fixture.Mock<IMovieDownloadService>(mock => mock.Setup(x => x.GetMovieByDownloadExternalIdAsync("10", CancellationToken.None))
            .ReturnsAsync(movie));
        var observed = await _fixture.Subject.GetActiveAsync("10");
        observed.Should().BeSameAs(movie.Download!);
    }

    [Fact]
    public async Task Searching()
    {
        var request = new PaginatedRequest {Page = 1, Limit = 2};
        var movies = FakeDto.MovieDto.Generate(2);
        var result = FakeDto.PaginatedData(movies);
        _fixture.Mock<IMovieService>(mock => mock.Setup(x => x.SearchAsync(It.IsAny<SearchMoviesRequest>(), CancellationToken.None))
            .ReturnsAsync(result));
        var observed = await _fixture.Subject.SearchAsync(request);

        observed.Should().BeEquivalentTo(new PaginatedData<MovieDownloadDto>
        {
            Page = result.Page,
            Limit = result.Limit,
            Count = result.Count,
            Data = movies.Select(x => x.Download!).ToList()
        });
    }
}