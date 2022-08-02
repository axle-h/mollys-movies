using System.IO.Abstractions;
using System.Net.Http;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MihaZupan;
using MollysMovies.Common;
using MollysMovies.Common.Health;
using MollysMovies.Common.Mongo;
using MollysMovies.Common.Scraper;
using MollysMovies.Common.TransmissionRpc;
using MollysMovies.Common.Validation;
using MollysMovies.Scraper;
using MollysMovies.Scraper.Health;
using MollysMovies.Scraper.Image;
using MollysMovies.Scraper.Movies;
using MollysMovies.Scraper.Plex;
using MollysMovies.Scraper.Yts;
using MollysMovies.ScraperClient;

var builder = WebApplication.CreateBuilder(args);

// Mongo
builder.Services.AddMongo();

// Health
builder.Services.AddHealthChecks()
    .AddProbe<ScraperHealthCheck>("yts")
    .AddProbe<ScraperHealthCheck>("plex")
    .AddProbe<TransmissionHealthCheck>("transmission");

// Masstransit
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((c, o) =>
    {
        o.Host(c.GetRequiredService<IConfiguration>().GetConnectionUrl("rabbitmq"));
        o.ConfigureEndpoints(c);
    });
    x.AddConsumers(typeof(Program).Assembly);
});

// Validation
builder.Services.AddFluentValidation(c =>
{
    c.RegisterValidatorsFromAssemblyContaining<Program>();
    c.ImplicitlyValidateChildProperties = true;
    c.DisableDataAnnotationsValidation = true;
});

builder.Services.AddOptions<ScraperOptions>()
    .BindConfiguration("Scraper")
    .ValidateFluentValidator().ValidateOnStart();

builder.Services.AddTransmissionRpcClient();

builder.Services
    .AddTransient<IImageScraperService, ImageScraperService>()
    .AddTransient<IScraperService, ScraperService>()
    .AddTorrentScraper<YtsScraper>()
    .AddSingleton<IYtsMapper, YtsMapper>()
    .AddLocalScraper<PlexScraper>()
    .AddSingleton<IPlexMapper, PlexMapper>()
    .AddTransient<IMovieService, MovieService>()
    .AddTransient<IMovieLibraryService, MovieLibraryService>()
    .AddTransient<IMovieDownloadCompleteService, MovieDownloadCompleteService>()
    .AddTransient<IMovieImageRepository, MovieImageRepository>()
    .AddTransient<IScrapeRepository, ScrapeRepository>()
    .AddTransient<IMovieRepository, MovieRepository>()
    .AddTransient<IScraperClient, MassTransitScraperClient>();

// Scraper API clients
var ytsClientBuilder = builder.Services.AddHttpClient<IYtsClient, YtsClient>((p, c) =>
{
    c.BaseAddress = p.GetRequiredService<IConfiguration>().GetConnectionUrl("yts");
});

if (builder.Configuration.TryGetApiUrl("proxy", out var proxyUri))
{
    // TODO log this
    ytsClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {Proxy = new HttpToSocks5Proxy(proxyUri.Host, proxyUri.Port)});
}

builder.Services.AddHttpClient<IPlexApiClient, PlexApiClient>((p, c) =>
{
    c.BaseAddress = p.GetRequiredService<IConfiguration>().GetConnectionUrl("plex");
});

// Common
builder.Services
    .AddSingleton<ISystemClock, SystemClock>()
    .AddSingleton<IFileSystem, FileSystem>();

var app = builder.Build();

app.MapProbes();

app.Run();

/// <summary>
///     Redefined implicit Program class as public for e2e testing.
/// </summary>
public partial class Program
{
}