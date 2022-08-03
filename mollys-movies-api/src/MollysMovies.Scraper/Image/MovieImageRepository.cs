using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MollysMovies.Scraper.Image;

public interface IMovieImageRepository
{
    Task<string> CreateMovieImageAsync(string imdbCode, byte[] content, string contentType, CancellationToken cancellationToken = default);

    string? GetMovieImage(string imdbCode);
}

public class MovieImageRepository : IMovieImageRepository
{
    private readonly IFileSystem _fileSystem;
    private readonly ScraperOptions _options;

    public MovieImageRepository(IOptions<ScraperOptions> options, IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _options = options.Value;
    }

    private IDirectoryInfo ImageDirectory => _fileSystem.Directory.CreateDirectory(_options.ImagePath);

    public async Task<string> CreateMovieImageAsync(string imdbCode, byte[] content, string contentType,
        CancellationToken cancellationToken = default)
    {
        var filename = imdbCode + contentType.Split('/').Last() switch
        {
            "jpeg" => ".jpg",
            "jpg" => ".jpg",
            "png" => ".png",
            _ => throw new ArgumentException($"unknown image mime type {contentType}")
        };

        var path = Path.Combine(ImageDirectory.FullName, filename);
        _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path));
        await _fileSystem.File.WriteAllBytesAsync(path, content, cancellationToken);

        return filename;
    }

    public string? GetMovieImage(string imdbCode)
    {
        var file = ImageDirectory.EnumerateFiles(imdbCode + ".*").FirstOrDefault();
        return file is null ? null : Path.GetFileName(file.Name);
    }
}