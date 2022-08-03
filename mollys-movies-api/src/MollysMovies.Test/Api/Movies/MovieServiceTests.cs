using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MollysMovies.Api.Common;
using MollysMovies.Api.Movies;
using MollysMovies.Api.Movies.Models;
using MollysMovies.FakeData;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Api.Movies;

public class MovieServiceTests : IClassFixture<AutoMockFixtureBuilder<MovieService>>
{
    private readonly AutoMockFixture<MovieService> _fixture;

    public MovieServiceTests(AutoMockFixtureBuilder<MovieService> builder)
    {
        _fixture = builder
            .InjectMock<IMovieRepository>()
            .InjectMock<IMovieMapper>()
            .Build();
    }

    [Fact]
    public async Task Getting_movie()
    {
        var movie = Fake.Movie.Generate();
        var dto = FakeDto.MovieDto.Generate();

        _fixture
            .Mock<IMovieMapper>(m => m.Setup(x => x.ToMovieDto(movie)).Returns(dto))
            .Mock<IMovieRepository>(m =>
                m.Setup(x => x.GetByImdbCodeAsync(movie.ImdbCode, CancellationToken.None))
                    .ReturnsAsync(movie));

        var observed = await _fixture.Subject.GetAsync(movie.ImdbCode);

        observed.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Searching()
    {
        var movies = Fake.Movie.Generate(2);
        var dtos = FakeDto.MovieDto.Generate(2);
        var request = FakeDto.SearchMoviesRequest.Generate();
        var query = FakeDto.PaginatedMovieQuery.Generate();
        var data = FakeDto.PaginatedData(movies);

        _fixture
            .Mock<IMovieMapper>(mock =>
            {
                mock.Setup(x => x.ToPaginatedMovieQuery(request)).Returns(query);

                foreach (var (movie, dto) in movies.Zip(dtos))
                {
                    mock.Setup(x => x.ToMovieDto(movie)).Returns(dto);
                }
            })
            .Mock<IMovieRepository>(mock =>
            {
                mock.Setup(x => x.SearchAsync(query, CancellationToken.None)).ReturnsAsync(data);
            });

        var observed = await _fixture.Subject.SearchAsync(request);

        using var scope = new AssertionScope();
        observed.Should().BeEquivalentTo(new PaginatedData<MovieDto>
        {
            Page = data.Page,
            Limit = data.Limit,
            Count = data.Count,
            Data = dtos
        });
    }

    [Fact]
    public async Task Getting_all_genres()
    {
        var genres = Fake.Faker.Genres().ToHashSet();
        _fixture.Mock<IMovieRepository>(mock => mock.Setup(x => x.GetAllGenresAsync(CancellationToken.None)).ReturnsAsync(genres));
        var observed = await _fixture.Subject.GetAllGenresAsync();
        observed.Should().BeSameAs(genres);
    }
}