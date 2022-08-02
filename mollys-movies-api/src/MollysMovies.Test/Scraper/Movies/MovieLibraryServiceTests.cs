using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.Common.TransmissionRpc;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Movies;
using MollysMovies.Test.Fixtures;
using Xunit;

namespace MollysMovies.Test.Scraper.Movies;

public class MovieLibraryServiceTests : IClassFixture<AutoMockFixtureBuilder<MovieLibraryService>>
{
    private readonly AutoMockFixture<MovieLibraryService> _fixture;

    private readonly TorrentInfo _torrent = FakeDto.TorrentInfo.Generate() with
    {
        DownloadDir = "/var/downloads",
        Name = "Interstellar (2014) [YTS]",
        Files = new List<TorrentFile>
        {
            FakeDto.TorrentFile.Generate() with {Name = "Interstellar (2014) [YTS]/interstellar.2014.h264.1080p.yts.mp4"},
            FakeDto.TorrentFile.Generate() with {Name = "Interstellar (2014) [YTS]/Subs/English.srt"},
            FakeDto.TorrentFile.Generate() with {Name = "Interstellar (2014) [YTS]/YTS.txt"} // junk: should be deleted
        }
    };

    public MovieLibraryServiceTests(AutoMockFixtureBuilder<MovieLibraryService> builder)
    {
        _fixture = builder
            .InjectFileSystem(f =>
            {
                f.AddDirectory("/var/downloads");
                f.AddDirectory("/var/movies");
            })
            .Services(services => services.Configure<ScraperOptions>(o =>
            {
                o.MovieLibraryPath = "/var/movies";
                o.DownloadsPath = "/var/downloads";
            }))
            .Build();
    }

    [Fact]
    public void Attempting_to_add_movie_but_no_movie_file()
    {
        var act = () => _fixture.Subject.AddMovie("Interstellar (2014)", _torrent with {Files = _torrent.Files.Skip(1).ToList()});
        act.Should().Throw<Exception>()
            .WithMessage("cannot determine movie file from torrent files: Interstellar (2014) [YTS]/Subs/English.srt, Interstellar (2014) [YTS]/YTS.txt");
    }
    
    [Fact]
    public void Attempting_to_add_movie_but_wrong_download_dir()
    {
        var act = () =>  _fixture.Subject.AddMovie("Interstellar (2014)", _torrent with {DownloadDir = "/somewhere-else"});
        act.Should().Throw<Exception>().WithMessage("expecting download in /var/downloads but is in /somewhere-else");
    }

    [Fact]
    public void Adding_movie()
    {
        var fileSystem = _fixture.FileSystem();
        for (byte i = 0; i < _torrent.Files.Count; i++)
        {
            fileSystem.AddFile("/var/downloads/" + _torrent.Files[i].Name, new MockFileData($"torrent-file-{i}"));
        }

        _fixture.Subject.AddMovie("Interstellar (2014)", _torrent);

        fileSystem.Should().ContainFile("/var/movies/Interstellar (2014)/Interstellar (2014).mp4", "torrent-file-0")
            .And.ContainFile("/var/movies/Interstellar (2014)/Interstellar (2014).srt", "torrent-file-1")
            .And.ContainDirectory("/var/downloads", "should not delete the downloads dir");
    }
}