using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MollysMovies.ScraperClient;

namespace MollysMovies.Callback;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await GetHostBuilder(args)
            .Build()
            .RunAsync();
    }

    public static IHostBuilder GetHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();
                    x.UsingRabbitMq((c, o) =>
                    {
                        o.Host(GetConnectionUrl(c.GetRequiredService<IConfiguration>(), "rabbitmq"));
                    });
                });

                services.AddOptions<MassTransitHostOptions>()
                    .Configure(options =>
                    {
                        options.WaitUntilStarted = true;
                    });

                services.AddOptions<TransmissionCallbackOptions>()
                    .BindConfiguration(string.Empty);

                services.AddSingleton<IScraperClient, MassTransitScraperClient>();
                services.AddHostedService<CallbackService>();
            });
    }
    
    private static Uri GetConnectionUrl(IConfiguration configuration, string name) =>
        Uri.TryCreate(configuration.GetConnectionString(name), UriKind.Absolute, out var uri)
            ? uri
            : throw new Exception($"valid {name} url is required");
}