using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MollysMovies.Api.E2e.Fixtures;
using MollysMovies.Common.Mongo;
using Xunit;

namespace MollysMovies.Api.E2e.Movies;

public class GetMovieTests : MollysMoviesApiTests
{
    public GetMovieTests(MollysMoviesApiFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Attempting_to_get_missing_movie()
    {
        var act = () => Fixture.MoviesApi.GetMovieAsync("abc123");
        await act.Should().ThrowApiExceptionAsync(404, ("", "cannot find Movie with keys {\"ImdbCode\":\"abc123\"}"));
    }

    [Fact]
    public async Task Getting_movie()
    {
        var movie = TestSeedData.Movie("Back to the Future");
        var observed = await Fixture.MoviesApi.GetMovieAsync(movie.ImdbCode);

        using var scope = new AssertionScope();

        observed.Should().BeEquivalentTo(movie, o => o.ComparingToDto());
    }
}