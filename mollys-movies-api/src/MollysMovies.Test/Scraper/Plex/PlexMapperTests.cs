using FluentAssertions;
using FluentAssertions.Execution;
using MollysMovies.Scraper.Plex;
using MollysMovies.Test.Fixtures;
using Xunit;

namespace MollysMovies.Test.Scraper.Plex;

public class PlexMapperTests
{
    private readonly PlexMapper _subject = new();

    [Fact]
    public void Mapping_from_PlexMovie_to_CreateLocalMovieRequest()
    {
        var movie = FakeDto.PlexMovie.Generate();
        var observed = _subject.ToCreateLocalMovieRequest(movie);

        using var scope = new AssertionScope();
        observed.ImdbCode.Should().Be(movie.ImdbCode);
        observed.Title.Should().Be(movie.Title);
        observed.Year.Should().Be(movie.Year);
        observed.DateCreated.Should().Be(movie.DateCreated);
        observed.ThumbPath.Should().Be(movie.ThumbPath);
    }
}