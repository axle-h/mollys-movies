using MakeMovies.Api.Meta.Tmdb.Three.Find.Item;

namespace MakeMovies.Api.Meta.Tmdb;

public class TmdbMetaSource(TmdbClient client) : IMetaSource
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private string? _imageBaseUrl;
    
    public int Priority => 1;
    
    public async Task<MovieMeta?> GetByImdbCodeAsync(string imdbCode, CancellationToken cancellationToken = default)
    {
        var imageBaseUrl = await ImageBaseUrlAsync(cancellationToken);
        var meta = await client.Three.Find[imdbCode]
            .GetAsync(x => x.QueryParameters.ExternalSource = GetExternal_sourceQueryParameterType.Imdb_id,
                cancellationToken);

        var result = meta?.MovieResults?.FirstOrDefault();
        if (result is null)
        {
            return null;
        }
        
        var imageUrl = result.PosterPath is null ? null : $"{imageBaseUrl}/w500/{result.PosterPath.TrimStart('/')}";
        return new MovieMeta(imdbCode, imageUrl);
    }

    private async Task<string> ImageBaseUrlAsync(CancellationToken cancellationToken)
    {
        if (_imageBaseUrl != null)
        {
            return _imageBaseUrl;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_imageBaseUrl != null)
            {
                return _imageBaseUrl;
            }
            
            var config = await client.Three.Configuration.GetAsync(cancellationToken: cancellationToken);
            var baseUrl = config?.Images?.SecureBaseUrl ?? throw new Exception("cannot get tmdb base url");

            _imageBaseUrl = baseUrl.TrimEnd('/');

            return _imageBaseUrl;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}