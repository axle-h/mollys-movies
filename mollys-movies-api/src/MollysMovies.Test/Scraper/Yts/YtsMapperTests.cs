using System;
using FluentAssertions;
using FluentAssertions.Execution;
using MollysMovies.Scraper.Yts;
using MollysMovies.Test.Fixtures;
using Xunit;

namespace MollysMovies.Test.Scraper.Yts;

public class YtsMapperTests
{
    private readonly YtsMapper _subject = new();

    [Fact]
    public void Mapping_from_YtsMovieSummary_to_CreateMovieRequest()
    {
        var source = FakeDto.YtsMovieSummary.Generate() with
        {
            Url = new Uri("https://yts.mx/movies/7-grandmasters-1977"),
            LargeCoverImage = new Uri("https://yts.mx/assets/images/movies/7_grandmasters_1977/large-cover.jpg")
        };
        var observed = _subject.ToCreateMovieRequest(source);

        using var scope = new AssertionScope();
        observed.ImdbCode.Should().Be(source.ImdbCode);
        observed.Title.Should().Be(source.Title);
        observed.Language.Should().Be(source.Language);
        observed.Year.Should().Be(source.Year);
        observed.Rating.Should().Be(source.Rating);
        observed.Description.Should().Be(source.DescriptionFull);
        observed.Genres.Should().BeEquivalentTo(source.Genres);
        observed.YouTubeTrailerCode.Should().Be(source.YtTrailerCode);
        observed.SourceCoverImageUrl.Should().Be("/assets/images/movies/7_grandmasters_1977/large-cover.jpg");
        observed.SourceUrl.Should().Be("/movies/7-grandmasters-1977");
        observed.SourceId.Should().Be(source.Id.ToString());
        observed.DateCreated.Should().Be(source.DateUploaded);
        observed.Torrents.Should().HaveSameCount(source.Torrents);
    }

    [Fact]
    public void Mapping_from_YtsTorrent_to_CreateTorrentRequest()
    {
        var source = FakeDto.YtsTorrent.Generate();
        var observed = _subject.ToCreateTorrentRequest(source);

        using var scope = new AssertionScope();
        observed.Url.Should().Be(source.Url.AbsoluteUri);
        observed.Hash.Should().Be(source.Hash);
        observed.Quality.Should().Be(source.Quality);
        observed.Type.Should().Be(source.Type);
        observed.SizeBytes.Should().Be(source.SizeBytes);
    }
}