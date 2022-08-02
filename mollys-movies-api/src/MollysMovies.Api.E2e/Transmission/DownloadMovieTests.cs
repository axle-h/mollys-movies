using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.E2e.Fixtures;
using MollysMovies.Common.Mongo;
using MollysMovies.Common.Movies;
using MollysMovies.FakeData;
using MongoDB.Driver;
using Xunit;

namespace MollysMovies.Api.E2e.Transmission;

public class DownloadMovieTests : MollysMoviesApiTests
{
    public DownloadMovieTests(MollysMoviesApiFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Attempting_to_download_movie_but_already_downloaded()
    {
        var movie = TestSeedData.Movie("Interstellar");
        var torrent = movie.Torrents.First();
        var act = () => Fixture.TorrentApi.DownloadMovieAsync(movie.ImdbCode, torrent.Quality!, torrent.Type!);
        await act.Should().ThrowApiExceptionAsync(400, ("", "movie with imdb code 'tt0816692' is already downloaded"));
    }

    [Fact]
    public async Task Attempting_to_download_movie_but_currently_downloading()
    {
        var movie = Fake.Movie
            .With(m =>
            {
                m.ImdbCode = "tt0983194";
                m.LocalSource = null;
            }).Generate();
        await Fixture.AddMoviesAsync(movie);

        var torrent = movie.Torrents.First();
        var act = () => Fixture.TorrentApi.DownloadMovieAsync(movie.ImdbCode, torrent.Quality!, torrent.Type!);
        await act.Should().ThrowApiExceptionAsync(400, ("", "movie with imdb code 'tt0983194' is already downloading"));
    }

    [Fact]
    public async Task Downloading_movie()
    {
        var torrent = Fake.Torrent.With(t =>
        {
            t.Hash = "abc123";
            t.Source = "yts";
        }).Generate();
        var movie = Fake.Movie
            .With(m =>
            {
                m.Meta!.Title = "Snowpiercer";
                m.Meta.Year = 2013;
                m.LocalSource = null;
                m.Download = null;
                m.Torrents = new List<Torrent> {torrent};
            })
            .Generate();
        await Fixture.AddMoviesAsync(movie);
        var externalId = Fake.Faker.Id();
        var magnetUri =
            "magnet:?xt=urn:btih:abc123&dn=Snowpiercer%20%282013%29&tr=udp://tracker1:1337/announce&tr=udp://tracker2:1337/announce";
        var torrentAddId = Guid.NewGuid();

        Fixture.WireMock
            // given torrent doesnt exist
            .GivenTransmissionRpc("torrent-get", new { },
                new {removed = Array.Empty<string>(), torrents = Array.Empty<string>()})
            // given torrent added
            .GivenTransmissionRpc("torrent-add", new {filename = magnetUri}, new Dictionary<string, object>
            {
                ["torrent-added"] = new {hashString = "abc123", id = externalId, name = "Snowpiercer (2013)"}
            }, torrentAddId);

        await Fixture.TorrentApi.DownloadMovieAsync(movie.ImdbCode, torrent.Quality!, torrent.Type!);

        var observedMovie = await Fixture.Movies
            .Find(x => x.ImdbCode == movie.ImdbCode)
            .FirstAsync();
        observedMovie.Download.Should().BeEquivalentTo(new MovieDownload
        {
            Name = "Snowpiercer (2013)",
            Quality = torrent.Quality,
            Source = "yts",
            Statuses = new List<MovieDownloadStatus> {new() {Status = MovieDownloadStatusCode.Started}},
            Type = torrent.Type,
            ExternalId = externalId.ToString(),
            MagnetUri = magnetUri
        }, o => o.ExcludingPropertiesOf<MovieDownload, MovieDownloadStatus>(x => x.DateCreated));

        Fixture.WireMock.Should().HaveCalledMapping(torrentAddId, "torrent should be added");
    }
}