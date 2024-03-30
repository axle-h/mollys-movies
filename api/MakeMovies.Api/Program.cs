using System.IO.Abstractions;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using MakeMovies.Api;
using MakeMovies.Api.Downloads;
using MakeMovies.Api.Downloads.TransmissionRpc;
using MakeMovies.Api.Health;
using MakeMovies.Api.Library;
using MakeMovies.Api.Library.Jellyfin;
using MakeMovies.Api.Meta;
using MakeMovies.Api.Meta.Omdb;
using MakeMovies.Api.Meta.Tmdb;
using MakeMovies.Api.Movies;
using MakeMovies.Api.Scrapes;
using MakeMovies.Api.Scrapes.Yts;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Host.UseSystemd();

// Health
builder.Services.AddHealthChecks()
    .AddProbe<TransmissionHealthCheck>("transmission")
    .AddProbe<JellyfinHealthCheck>("jellyfin")
    .AddProbe<TmdbHealthCheck>("tmdb")
    .AddProbe<OmdbHealthCheck>("omdb")
    .AddProbe<YtsHealthCheck>("yts");

builder.Services
    .AddSingleton<IFileSystem, FileSystem>()
    .AddSingleton<Db>()
    .AddHostedService(p => p.GetRequiredService<Db>())
    .AddOptions<DbOptions>()
    .BindConfiguration("Db");

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddRouting(o => { o.LowercaseUrls = true; });

builder.Services.AddSwaggerGen(c => {
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
    c.CustomSchemaIds(type => type.DefaultSchemaIdSelector());
    c.MapType<TimeSpan>(() => new OpenApiSchema
    {
        Type = "string",
        Example = new OpenApiString("12:15:01")
    });
    c.DescribeAllParametersInCamelCase();
});

// Download
builder.Services.AddHttpClient<ITransmissionRpcClient, TransmissionRpcClient>();
builder.Services
    .AddSingleton<IDownloadRepository, DownloadRepository>()
    .AddSingleton<TorrentService>()
    .AddHostedService<TorrentService>(p => p.GetRequiredService<TorrentService>())
    .AddSingleton<ITorrentService>(p => p.GetRequiredService<TorrentService>())
    .AddOptions<DownloadOptions>()
    .BindConfiguration("Download");

// Library
builder.Services.AddHttpClient<JellyfinClient>();
builder.Services
    .AddSingleton<ILibrarySource>(p => p.GetRequiredService<JellyfinClient>())
    .AddSingleton<ILibraryService, LibraryService>()
    .AddOptions<LibraryOptions>()
    .BindConfiguration("Library");

// Movies
builder.Services
    .AddSingleton<IMovieRepository, InMemoryMovieRepository>();

// Meta
builder.Services
    .AddSingleton<IMetaSource>(p => p.GetRequiredService<OmdbClient>())
    .AddHttpClient<OmdbClient>();
builder.Services
    .AddSingleton(provider => TmdbClientFactory.Build(provider.GetRequiredService<IOptions<MetaOptions>>()))
    .AddSingleton<TmdbMetaSource>()
    .AddSingleton<IMetaSource, TmdbMetaSource>()
    .AddSingleton<IMetaService, MetaService>()
    .AddOptions<MetaOptions>()
    .BindConfiguration("Meta");

// Scraper
builder.Services.AddSingleton<IScraper, YtsScraper>()
    .AddSingleton<IRootScraper, RootScraper>()
    .AddSingleton<IScrapeRepository, JsonScrapeRepository>()
    .AddSingleton<IScrapeService, ScrapeService>()
    .AddOptions<ScrapeOptions>()
    .BindConfiguration("Scrape");

builder.Services.AddHttpClient<IYtsClient, YtsClient>()
    .ConfigurePrimaryHttpMessageHandler(p =>
    {
        var proxyUrl = p.GetRequiredService<IOptions<ScrapeOptions>>().Value.ProxyUrl;
        if (proxyUrl is null)
        {
            return new HttpClientHandler();
        }
        p.GetRequiredService<ILogger<Program>>().LogInformation("using proxy {url}", proxyUrl);
        return new SocketsHttpHandler { Proxy = new WebProxy(proxyUrl) };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var imagePath = app.Services.GetRequiredService<IOptions<MetaOptions>>().Value.ImagePath;
if (!Path.IsPathRooted(imagePath))
{
    imagePath = Path.Join(Directory.GetCurrentDirectory(), imagePath);
}
Directory.CreateDirectory(imagePath);
app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(imagePath),
    RequestPath = "/movie-images"
});

app.UseCors(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.MapControllers();
app.MapProbes();

app.Run();

public partial class Program { }