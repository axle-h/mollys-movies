using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MollysMovies.Api.E2e.Fixtures;
using MollysMovies.Client.Model;
using MollysMovies.FakeData;
using Xunit;

namespace MollysMovies.Api.E2e.Transmission;

public class GetDownloadByExternalIdTests : MollysMoviesApiTests
{
    public GetDownloadByExternalIdTests(MollysMoviesApiFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Attempting_to_get_missing_download()
    {
        var act = () => Fixture.TransmissionApi.GetDownloadByExternalIdAsync("9999");
        await act.Should().ThrowApiExceptionAsync(404, ("", "cannot find Movie with keys {\"ExternalDownloadId\":\"9999\"}"));
    }

    [Fact]
    public async Task Attempting_to_get_complete_download()
    {
        var act = () => Fixture.TransmissionApi.GetDownloadByExternalIdAsync("15");
        await act.Should().ThrowApiExceptionAsync(404, ("", "cannot find MovieDownload with keys {\"ExternalId\":\"15\"}, context is not active"));
    }

    [Fact]
    public async Task Getting_started_download()
    {
        var download = Fake.MovieDownload.Generate("default,Started");
        var movie = Fake.Movie.With(m =>
        {
            m.LocalSource = null;
            m.Download = download;
        }).Generate();
        await Fixture.AddMoviesAsync(movie);

        var observed = await Fixture.TransmissionApi.GetDownloadByExternalIdAsync(download.ExternalId!);

        using var scope = new AssertionScope();
        observed.Should().BeEquivalentTo(download, o => o
            .Excluding(x => x.Statuses)
            .Excluding(x => x.MagnetUri)
            .Excluding(x => x.Source)
            .Excluding(x => x.Quality)
            .Excluding(x => x.Type));
        observed.Status.Should().Be(MovieDownloadStatusCode.Started);
    }
}