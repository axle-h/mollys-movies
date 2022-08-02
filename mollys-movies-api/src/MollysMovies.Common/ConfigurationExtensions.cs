using Microsoft.Extensions.Configuration;

namespace MollysMovies.Common;

public static class ConfigurationExtensions
{
    public static Uri GetConnectionUrl(this IConfiguration configuration, string name) =>
        configuration.TryGetApiUrl(name, out var uri)
            ? uri
            : throw new Exception($"valid {name} url is required");

    public static bool TryGetApiUrl(this IConfiguration configuration, string name, out Uri uri) =>
        Uri.TryCreate(configuration.GetConnectionString(name), UriKind.Absolute, out uri!);
}