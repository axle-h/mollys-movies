using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace MollysMovies.Api.MovieImages;

public interface IMovieImageFileProviderFactory
{
    IFileProvider Build();
}

public class MovieImageFileProviderFactory : IMovieImageFileProviderFactory
{
    private readonly MovieImageOptions _imageOptions;

    public MovieImageFileProviderFactory(IOptions<MovieImageOptions> options)
    {
        _imageOptions = options.Value;
    }
    
    public IFileProvider Build() => new PhysicalFileProvider(Path.GetFullPath(_imageOptions.Path));
}