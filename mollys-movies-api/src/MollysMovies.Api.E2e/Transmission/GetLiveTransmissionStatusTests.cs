using System;
using System.Threading.Tasks;
using FluentAssertions;
using MollysMovies.Api.E2e.Fixtures;
using MollysMovies.Client.Model;
using MollysMovies.Common.Mongo;
using MollysMovies.FakeData;
using Xunit;

namespace MollysMovies.Api.E2e.Transmission;

public class GetLiveTransmissionStatusTests : MollysMoviesApiTests
{
    public GetLiveTransmissionStatusTests(MollysMoviesApiFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Getting_completed_torrent_status()
    {
        var movie = TestSeedData.Movie("Source Code");
        var observed = await Fixture.TorrentApi.GetLiveTransmissionStatusAsync(movie.ImdbCode);
        observed.Should().BeEquivalentTo(new LiveTransmissionStatus("Source Code (2011)", true));
    }

    [Fact]
    public async Task Getting_downloaded_torrent_status()
    {
        var download = Fake.MovieDownload.Generate("default,Downloaded");
        var movie = Fake.Movie
            .With(m =>
            {
                m.LocalSource = null;
                m.Download = download;
            })
            .Generate();
        await Fixture.AddMoviesAsync(movie);
        var observed = await Fixture.TorrentApi.GetLiveTransmissionStatusAsync(movie.ImdbCode);
        observed.Should().BeEquivalentTo(new LiveTransmissionStatus(download.Name, true));
    }

    [Fact]
    public async Task Getting_started_torrent_status()
    {
        var download = Fake.MovieDownload
            .With(d => d.ExternalId = "66666")
            .Generate("default,Started");
        var movie = Fake.Movie
            .With(m =>
            {
                m.LocalSource = null;
                m.Download = download;
            })
            .Generate();
        await Fixture.AddMoviesAsync(movie);

        Fixture.WireMock.GivenTransmissionRpc("torrent-get",
            new {ids = new[] {66666}},
            new
            {
                removed = Array.Empty<string>(),
                torrents = new[] {new {id = 66666, percentDone = 0.85, isStalled = false, eta = 120}}
            });

        var observed = await Fixture.TorrentApi.GetLiveTransmissionStatusAsync(movie.ImdbCode);
        observed.Should()
            .BeEquivalentTo(new LiveTransmissionStatus(download.Name, false, false, 120, 0.85));
    }
}