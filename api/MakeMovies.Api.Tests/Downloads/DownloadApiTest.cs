using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions.Execution;
using MakeMovies.Api.Downloads;
using MakeMovies.Api.Downloads.TransmissionRpc;
using MakeMovies.Api.Tests.Downloads.TransmissionRpc;
using MakeMovies.Api.Tests.Library.Jellyfin;

namespace MakeMovies.Api.Tests.Downloads;

public class DownloadApiTest(ApiFixture fixture) : ApiTests(fixture)
{
    [Fact]
    public async Task Attempting_to_download_missing_movie()
    {
        var response = await Fixture.CreateClient().PostAsync("/api/v1/movie/missing/download", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("cannot find movie with id 'missing'");
    }
    
    [Fact]
    public async Task Attempting_to_download_movie_already_in_library()
    {
        var response = await Fixture.CreateClient().PostAsync("/api/v1/movie/yts_1632/download", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("movie with id 'yts_1632' is already downloaded");
    }
    
    [Fact]
    public async Task Attempting_to_download_movie_currently_downloading()
    {
        var movie = Fake.Movie with { Id = "downloading_movie" };
        await Fixture.Db.Movies.UpsertAsync(movie.Id, movie);
        
        var download = Fake.Download with { MovieId = "downloading_movie" };
        await Fixture.Db.Downloads.UpsertAsync(download.Id, download);
        
        var response = await Fixture.CreateClient().PostAsync("/api/v1/movie/downloading_movie/download", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("movie with id 'downloading_movie' is currently downloading");
    }
    
    [Fact]
    public async Task Attempting_to_download_movie_without_an_acceptable_torrent()
    {
        var movie = Fake.Movie with { Id = "bad_movie", Torrents = [Fake.Torrent with { Quality = "dog" }] };
        await Fixture.Db.Movies.UpsertAsync(movie.Id, movie);
        
        var response = await Fixture.CreateClient().PostAsync("/api/v1/movie/bad_movie/download", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("cannot download movie with id = 'bad_movie', no acceptable torrents available");
    }
    
    [Fact(Timeout = 30000)]
    public async Task Movie_download_e2e()
    {
        var movie = Fake.Movie with { Id = "download_movie" };
        await Fixture.Db.Movies.UpsertAsync("download_movie", movie);

        var addTorrentUuid = Fixture.WireMock
            .GivenTransmissionStatus(123, 0.5, 30)
            .GivenHappyTransmissionStateForDownload();

        var client = Fixture.CreateClient();
        
        // 1. start the download
        var startDownloadResponse = await client
            .PostAsync("/api/v1/movie/download_movie/download", null);
        startDownloadResponse.EnsureSuccessStatusCode();
        Fixture.WireMock.Should().HaveCalledMapping(addTorrentUuid, "torrent should be added");

        // 2. wait until the download progress is updated from transmission
        while (true)
        {
            await Task.Delay(2000);
            var download = await GetDownloadAsync();

            if (download.Stats is null)
            {
                continue;
            }

            download.Stats.Should().BeEquivalentTo(
                new DownloadStats("Some Movie (2024) [YTS]", 0.5, false, TimeSpan.FromSeconds(30), TransmissionWireMock.Files.ToHashSet()));
            break;
        }


        // 3. complete the download by placing the files in the download folder and removing the torrent from transmission
        var downloadDir = Path.Join(Fixture.TmpPath, "downloads");
        var movieDownloadDir = Path.Join(downloadDir, "Some Movie (2024) [YTS]");
        Directory.CreateDirectory(movieDownloadDir);
        foreach (var file in TransmissionWireMock.Files)
        {
            await File.WriteAllTextAsync(Path.Join(downloadDir, file), file);            
        }
        var updateLibraryUuid = Fixture.WireMock
            .GivenMissingTransmissionStatus(123)
            .GivenJellyfinUpdate();
        
        while (true)
        {
            await Task.Delay(2000);
            var download = await GetDownloadAsync();

            if (!download.Complete)
            {
                continue;
            }
            
            break;
        }
        
        Fixture.WireMock.Should()
            .HaveCalledMapping(updateLibraryUuid, "jellyfin library should have been updated");

        using var scope = new AssertionScope();
        var moviesDir = Path.Join(Fixture.TmpPath, "movies", "Some Movie (2024)");
        Directory.EnumerateFiles(moviesDir).Select(Path.GetFileName).ToList().Should()
            .HaveCount(2).And.Contain("Some Movie (2024).mp4", "Some Movie (2024).srt");
        File.ReadAllText(Path.Join(moviesDir, "Some Movie (2024).mp4")).Should().Be("Some Movie (2024) [YTS]/movie.mp4");
        File.ReadAllText(Path.Join(moviesDir, "Some Movie (2024).srt")).Should().Be("Some Movie (2024) [YTS]/subtitles.srt");

        Directory.Exists(movieDownloadDir).Should().BeFalse();


        Fixture.Db.Movies.Get("download_movie")!.InLibrary.Should().BeTrue();
    }
    
    private async Task<Download> GetDownloadAsync()
    {
        var response = await Fixture.CreateClient().GetAsync("/api/v1/download");
        response.EnsureSuccessStatusCode();

        var downloads = await response.Content.ReadFromJsonAsync<PaginatedData<Download>>();
        return downloads!.Data.First(d => d.MovieId == "download_movie");
    }
}