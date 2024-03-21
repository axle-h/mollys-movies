using System.IO.Abstractions;
using MakeMovies.Api.Downloads;
using MakeMovies.Api.Downloads.TransmissionRpc;
using Microsoft.Extensions.Options;

namespace MakeMovies.Api.Library;

public interface ILibraryService
{
    Task<ISet<string>> AllImdbCodesAsync(CancellationToken cancellationToken = default);
    
    void MoveDownloadedMovieIntoLibrary(Download download);
    
    Task UpdateLibraryAsync(CancellationToken cancellationToken = default);
}

public class LibraryService(
    ILibrarySource librarySource,
    IFileSystem fileSystem,
    ILogger<LibraryService> logger,
    IOptions<LibraryOptions> options) : ILibraryService
{
    private static readonly string[] MovieExtensions = [".avi", ".mp4", ".mkv"];
    private static readonly string[] SubtitlesExtensions = [".srt", ".sub", ".idx"];
    
    public async Task<ISet<string>> AllImdbCodesAsync(CancellationToken cancellationToken = default)
    {
        var libraryMovies = await librarySource.ListAllAsync(cancellationToken);
        return libraryMovies.Select(x => x.ImdbCode).ToHashSet();
    }

    public void MoveDownloadedMovieIntoLibrary(Download download)
    {
        var stats = download.Stats;
        if (stats is null)
        {
            throw new Exception($"cannot complete download {download.Name} as it has no torrent info attached");
        }

        // determine source file types
        var downloadPath = options.Value.DownloadsPath;
        var infos = stats.Files
            .Select(file => fileSystem.FileInfo.New(fileSystem.Path.Combine(downloadPath, file)))
            .ToLookup(info => GetFileType(info.Extension));

        var movieFiles = infos[FileType.Movie].ToList();
        if (movieFiles.Count is 0 or > 1)
        {
            throw new Exception($"cannot determine movie file from torrent files: {string.Join(", ", stats.Files)}");
        }

        var movieFile = movieFiles[0];
        var bareMovieFile = fileSystem.Path.GetFileNameWithoutExtension(movieFile.FullName);

        var allSubtitleFiles = infos[FileType.Subtitles].ToList();
        var bestSubTitleFile = allSubtitleFiles.FirstOrDefault(info =>
                               {
                                   var tokens = fileSystem.Path.GetFileNameWithoutExtension(info.Name)
                                       .ToLower()
                                       .Split([' ', '.', '-', '_']);

                                   if (tokens.Contains("sdh") || tokens.Contains("hi"))
                                   {
                                       // ignore SDH (subtitles for deaf and hard of hearing) and HI (hearing impaired)
                                       return false;
                                   }

                                   return tokens.Any(x => x is "english" or "eng" or "en");
                               })
                               ?? allSubtitleFiles.FirstOrDefault(info =>
                                   fileSystem.Path.GetFileNameWithoutExtension(info.Name).Equals(bareMovieFile,
                                       StringComparison.OrdinalIgnoreCase))
                               ?? allSubtitleFiles.FirstOrDefault();


        // create library path
        var movieLibraryPath = fileSystem.Path.Combine(options.Value.MovieLibraryPath, download.Name);
        fileSystem.Directory.CreateDirectory(movieLibraryPath);

        MoveFile(movieFile);
        if (bestSubTitleFile is not null)
        {
            MoveFile(bestSubTitleFile);
        }

        // clean up remaining junk
        var downloadDirectory = fileSystem.Path.Join(downloadPath, stats.Name);
        logger.LogInformation("removing all junk files {JunkFiles} in {DownloadDirectory}",
            string.Join(", ", infos[FileType.Junk].Select(x => $"\"{x}\"")),
            downloadDirectory);
        fileSystem.Directory.Delete(downloadDirectory, recursive: true);
        return;

        void MoveFile(IFileSystemInfo src)
        {
            var destination = fileSystem.Path.Combine(movieLibraryPath,
                fileSystem.Path.ChangeExtension(download.Name, src.Extension));
            var destinationExists = fileSystem.File.Exists(destination);
            if (!src.Exists && destinationExists)
            {
                // probably a retry
                logger.LogInformation("ignoring already moved: file {From} -> {To}", src.FullName, destination);
                return;
            }

            if (destinationExists)
            {
                logger.LogWarning("ignoring already exists: file {From} -> {To}", src.FullName, destination);
                return;
            }

            logger.LogInformation("moving file {From} -> {To}", src.FullName, destination);
            fileSystem.File.Move(src.FullName, destination);
        }
    }

    public async Task UpdateLibraryAsync(CancellationToken cancellationToken = default)
    {
        await librarySource.UpdateLibraryAsync(cancellationToken);
    }

    private static FileType GetFileType(string extension) =>
        MovieExtensions.Contains(extension)
            ? FileType.Movie
            : SubtitlesExtensions.Contains(extension)
                ? FileType.Subtitles
                : FileType.Junk;

    private enum FileType { Junk, Movie, Subtitles }
}