using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MakeMovies.Api.Downloads.TransmissionRpc;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddTransmissionRpcClient(this IServiceCollection services) =>
        services.AddHttpClient<ITransmissionRpcClient, TransmissionRpcClient>().ConfigurePrimaryHttpMessageHandler(() =>
            new TransmissionRpcClient.TransmissionRpcHandler(new HttpClientHandler {AllowAutoRedirect = false}));

}