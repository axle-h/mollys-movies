using Microsoft.Extensions.Options;

namespace MakeMovies.Api.Meta.Omdb;

public class OmdbClient : IMetaSource
{
    private readonly HttpClient _client;
    private readonly OmdbOptions _options;

    public OmdbClient(HttpClient client, IOptions<MetaOptions> options)
    {
        _client = client;
        _options = options.Value.Omdb;
        _client.BaseAddress = _options.Url ?? throw new Exception("OMDB url is required");
    }

    public int Priority => 0;

    public async Task<MovieMeta?> GetByImdbCodeAsync(string imdbCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var movie = await GetAsync(imdbCode, cancellationToken);
            return movie is null ? null : new MovieMeta(imdbCode, movie.Poster);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<OmdbMovie?> GetAsync(string imdbCode, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"?i={imdbCode}&apikey={_options.ApiKey}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OmdbMovie>(cancellationToken);
    }
    
    private record OmdbMovie(string Poster);
}