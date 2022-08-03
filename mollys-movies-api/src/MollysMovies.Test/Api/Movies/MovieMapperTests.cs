using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using MollysMovies.Api.Common;
using MollysMovies.Api.Movies;
using MollysMovies.Api.Movies.Models;
using MollysMovies.Api.Movies.Requests;
using MollysMovies.Common.Movies;
using MollysMovies.FakeData;
using MollysMovies.Test.Fixtures;
using Xunit;

namespace MollysMovies.Test.Api.Movies;

public class MovieMapperTests
{
    private readonly MovieMapper _subject = new();

    [Fact]
    public void Mapping_from_SearchMoviesRequest_to_PaginatedMovieQuery()
    {
        var request = FakeDto.SearchMoviesRequest.Generate() with
        {
            OrderBy = MoviesOrderBy.Rating, OrderByDescending = true
        };
        var observed = _subject.ToPaginatedMovieQuery(request);

        using var scope = new AssertionScope();
        observed.Text.Should().Be(request.Title);
        observed.Quality.Should().Be(request.Quality);
        observed.Language.Should().Be(request.Language);
        observed.Downloaded.Should().Be(request.Downloaded);
        observed.Genre.Should().Be(request.Genre);
        observed.YearFrom.Should().Be(request.YearFrom);
        observed.YearTo.Should().Be(request.YearTo);
        observed.RatingFrom.Should().Be(request.RatingFrom);
        observed.RatingTo.Should().Be(request.RatingTo);
        observed.Page.Should().Be(request.Page);
        observed.Limit.Should().Be(request.Limit);
        observed.OrderBy.Should().BeEquivalentTo(new List<PaginatedOrderBy<Movie>>
        {
            new(x => x.Meta!.Rating, true)
        });
    }

    [Fact]
    public void Mapping_from_Movie_to_MovieDto()
    {
        var movie = Fake.Movie
            .With(m =>
            {
                m.Download = Fake.MovieDownload.Generate("default,Complete");
                m.Torrents = Fake.Torrent.Generate(1);
            })
            .Generate();
        var torrent = movie.Torrents.First();

        var observed = _subject.ToMovieDto(movie);

        using var scope = new AssertionScope();
        observed.ImdbCode.Should().Be(movie.ImdbCode);
        observed.Title.Should().Be(movie.Meta!.Title);
        observed.Language.Should().Be(movie.Meta!.Language);
        observed.Year.Should().Be(movie.Meta!.Year);
        observed.Rating.Should().Be(movie.Meta!.Rating);
        observed.Description.Should().Be(movie.Meta!.Description);
        observed.YouTubeTrailerCode.Should().Be(movie.Meta!.YouTubeTrailerCode);
        observed.ImageFilename.Should().Be(movie.Meta!.ImageFilename);
        observed.ImageFilename.Should().Be(movie.Meta!.ImageFilename);
        observed.Genres.Should().BeEquivalentTo(movie.Meta.Genres);
        observed.Torrents.Should().HaveCount(1).And.ContainEquivalentOf(torrent);
        observed.LocalSource.Should().Be(new LocalMovieSourceDto(movie.LocalSource!.Source!, movie.LocalSource!.DateCreated, movie.LocalSource!.DateScraped));
        observed.Download.Should().Be(new MovieDownloadDto(movie.ImdbCode, movie.Download!.ExternalId!, movie.Download.Name!, MovieDownloadStatusCode.Complete));
    }

    [Fact]
    public void Mapping_from_Movie_to_MovieDto_when_not_downloaded()
    {
        var movie = Fake.Movie.With(m =>
        {
            m.LocalSource = null;
            m.Download = null;
        }).Generate();

        var observed = _subject.ToMovieDto(movie);

        using var scope = new AssertionScope();
        observed.LocalSource.Should().BeNull();
        observed.Download.Should().BeNull();
    }
}