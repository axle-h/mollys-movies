using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MollysMovies.Common.TransmissionRpc;

namespace MollysMovies.Scraper.Movies;

public interface IMovieLibraryService
{
    void AddMovie(string name, TorrentInfo torrent);
}

public class MovieLibraryService : IMovieLibraryService
{
    private static readonly string[] MovieExtensions = {".avi", ".mp4", ".mkv"};
    private static readonly string[] SubtitlesExtensions = {".srt", ".sub", ".idx"};
    private readonly IFileSystem _fileSystem;

    private readonly ILogger<MovieLibraryService> _logger;
    private readonly ScraperOptions _options;

    public MovieLibraryService(
        IFileSystem fileSystem,
        IOptions<ScraperOptions> options,
        ILogger<MovieLibraryService> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _options = options.Value;
    }

    public void AddMovie(string name, TorrentInfo torrent)
    {
        if (torrent.DownloadDir.Trim() != _options.DownloadsPath)
        {
            throw new Exception($"expecting download in {_options.DownloadsPath} but is in {torrent.DownloadDir}");
        }
        
        // determine source file types
        var files = torrent.Files
            .Select(t => (path: Path.Combine(torrent.DownloadDir, t.Name), extension: Path.GetExtension(t.Name)))
            .Select(t => (t.path, t.extension, type: GetFileType(t.extension)))
            .ToList();

        if (!files.Any())
        {
            throw new Exception($"torrent has no files '{name}'");
        }
        
        if (files.All(x => x.type != FileType.Movie))
        {
            throw new Exception($"cannot determine movie file from torrent files: {string.Join(", ", torrent.Files.Select(x => x.Name))}");
        }

        // create library path
        var movieLibraryPath = Path.Combine(_options.MovieLibraryPath, name);
        _fileSystem.Directory.CreateDirectory(movieLibraryPath);

        // move all files into library
        foreach (var (path, extension, type) in files.Where(x => x.type != FileType.Junk))
        {
            var destination = Path.Combine(movieLibraryPath, Path.ChangeExtension(name, extension));
            _logger.LogInformation("moving {Type} file {From} -> {To}", type, path, destination);
            _fileSystem.File.Move(path, destination);
        }

        // clean up remaining junk
        var downloadDirectory = Path.Join(torrent.DownloadDir, torrent.Name);
        _logger.LogInformation("removing all junk files {JunkFiles} in {DownloadDirectory}",
            string.Join(", ", files.Where(x => x.type == FileType.Junk)
                .Select(x => $"\"{x}\"")),
            downloadDirectory);
        _fileSystem.Directory.Delete(downloadDirectory, true);
    }

    private static FileType GetFileType(string extension)
    {
        if (MovieExtensions.Contains(extension))
        {
            return FileType.Movie;
        }

        return SubtitlesExtensions.Contains(extension) ? FileType.Subtitles : FileType.Junk;
    }

    private enum FileType { Junk, Movie, Subtitles }
}