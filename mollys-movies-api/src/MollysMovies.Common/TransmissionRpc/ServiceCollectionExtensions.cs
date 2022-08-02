using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MollysMovies.Common.TransmissionRpc;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddTransmissionRpcClient(this IServiceCollection services) =>
        services.AddHttpClient<ITransmissionRpcClient, TransmissionRpcClient>((p, c) =>
        {
            c.BaseAddress = p.GetRequiredService<IConfiguration>().GetConnectionUrl("transmission");
        }).ConfigurePrimaryHttpMessageHandler(() =>
            new TransmissionRpcClient.TransmissionRpcHandler(new HttpClientHandler {AllowAutoRedirect = false}));

    private static Uri GetConnectionUrl(this IConfiguration configuration, string name) =>
        configuration.TryGetApiUrl(name, out var uri)
            ? uri
            : throw new Exception($"valid {name} url is required");

    private static bool TryGetApiUrl(this IConfiguration configuration, string name, out Uri uri) =>
        Uri.TryCreate(configuration.GetConnectionString(name), UriKind.Absolute, out uri!);
}