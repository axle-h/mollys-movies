using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace MakeMovies.Api.Meta.Tmdb;

public static class TmdbClientFactory
{
    public static TmdbClient Build(IOptions<MetaOptions> options)
    {
        var tmdbOptions = options.Value.Tmdb;
        
        IAuthenticationProvider authProvider = tmdbOptions.AccessToken == null
            ? new AnonymousAuthenticationProvider()
            : new BaseBearerTokenAuthenticationProvider(new StaticAccessTokenProvider(tmdbOptions.AccessToken));
        var adapter = new HttpClientRequestAdapter(authProvider);

        adapter.BaseUrl = tmdbOptions.Url?.ToString();
        
        return new TmdbClient(adapter);
    }

    private class StaticAccessTokenProvider(string token) : IAccessTokenProvider
    {
        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(token);

        public AllowedHostsValidator AllowedHostsValidator { get; } = new();
    }
}