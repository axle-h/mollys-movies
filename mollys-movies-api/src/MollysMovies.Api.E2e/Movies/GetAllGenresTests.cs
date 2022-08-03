using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.E2e.Fixtures;
using MollysMovies.Common.Mongo;
using Xunit;

namespace MollysMovies.Api.E2e.Movies;

public class GetAllGenresTests : MollysMoviesApiTests
{
    public GetAllGenresTests(MollysMoviesApiFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Getting_all_genres()
    {
        var observed = await Fixture.GenreApi.GetAllGenresAsync();
        var expected = TestSeedData.Movies.SelectMany(x => x.Meta?.Genres ?? new HashSet<string>()).ToList();
        observed.Should().NotBeEmpty().And.Contain(expected);
    }
}