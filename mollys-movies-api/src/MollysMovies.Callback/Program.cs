// remove torrent

using System;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MollysMovies.Callback;
using MollysMovies.ScraperClient;

static Uri GetConnectionUrl(IConfiguration configuration, string name) =>
    Uri.TryCreate(configuration.GetConnectionString(name), UriKind.Absolute, out var uri)
        ? uri
        : throw new Exception($"valid {name} url is required");

await Host.CreateDefaultBuilder(args)
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

        services.AddTransient<IScraperClient, MassTransitScraperClient>();
        services.AddHostedService<CallbackService>();
    })
    .Build()
    .RunAsync();
