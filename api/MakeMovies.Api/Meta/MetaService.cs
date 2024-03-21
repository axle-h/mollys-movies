using System.IO.Abstractions;
using Microsoft.Extensions.Options;

namespace MakeMovies.Api.Meta;

public interface IMetaService
{
    Task<string?> GetImageAsync(string imdbCode, CancellationToken cancellationToken = default);
}

public class MetaService : IMetaService
{
    private readonly HttpClient _imageClient;
    private readonly List<IMetaSource> _metaSources;
    private readonly IFileSystem _fileSystem;
    private readonly MetaOptions _options;
    
    public MetaService(HttpClient imageClient, IEnumerable<IMetaSource> metaSources, IFileSystem fileSystem, IOptions<MetaOptions> options)
    {
        _imageClient = imageClient;
        _metaSources = metaSources.OrderByDescending(x => x.Priority).ToList();
        _fileSystem = fileSystem;
        _options = options.Value;
        fileSystem.Directory.CreateDirectory(_options.ImagePath);
    }

    public async Task<string?> GetImageAsync(string imdbCode, CancellationToken cancellationToken = default) =>
        _fileSystem.Directory.GetFiles(_options.ImagePath, $"{imdbCode}.*")
            .Select(f => _fileSystem.Path.GetFileName(f))
            .FirstOrDefault()
        ?? await DownloadImageAsync(imdbCode, cancellationToken);

    private async Task<string?> GetImageUrl(string imdbCode, CancellationToken cancellationToken)
    {
        foreach (var metaSource in _metaSources)
        {
            var meta = await metaSource.GetByImdbCodeAsync(imdbCode, cancellationToken);
            if (meta?.ImageUrl is not null)
            {
                return meta.ImageUrl;
            }
        }

        return null;
    }
    
    private async Task<string?> DownloadImageAsync(string imdbCode, CancellationToken cancellationToken)
    {
        var imageUrl = await GetImageUrl(imdbCode, cancellationToken);
        if (imageUrl is null)
        {
            return null;
        }

        var stream = await _imageClient.GetStreamAsync(imageUrl, cancellationToken);
        var imageFile = GetImageFile(imageUrl, imdbCode);
        var imagePath = _fileSystem.Path.Join(_options.ImagePath, imageFile);
        
        await using var file = _fileSystem.File.OpenWrite(imagePath);
        await stream.CopyToAsync(file, cancellationToken);

        return imageFile;
    }

    private string GetImageFile(string url, string imdbCode)
    {
        var queryIndex = url.LastIndexOf('?');
        var strippedUrl = queryIndex < 0 ? url : url[..queryIndex];
        return imdbCode + _fileSystem.Path.GetExtension(strippedUrl);
    }
}