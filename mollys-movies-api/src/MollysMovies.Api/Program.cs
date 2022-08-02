using System.IO.Abstractions;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MollysMovies.Api.Common.Exceptions;
using MollysMovies.Api.Common.Routing;
using MollysMovies.Api.MovieImages;
using MollysMovies.Api.Movies;
using MollysMovies.Api.Scraper;
using MollysMovies.Api.Transmission;
using MollysMovies.Common;
using MollysMovies.Common.Health;
using MollysMovies.Common.Mongo;
using MollysMovies.Common.TransmissionRpc;
using MollysMovies.Common.Validation;
using MollysMovies.ScraperClient;

var builder = WebApplication.CreateBuilder(args);

// Mongo
builder.Services.AddMongo();

// Health
builder.Services.AddHealthChecks()
    .AddProbe<TransmissionHealthCheck>("transmission");

// Masstransit
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((c, o) =>
    {
        o.Host(c.GetRequiredService<IConfiguration>().GetConnectionUrl("rabbitmq"));
    });
}).AddMassTransitHostedService();

// API
builder.Services.AddControllers(o =>
    {
        o.Filters.Add<ApiExceptionFilter>();
        o.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .AddFluentValidation(c =>
    {
        c.RegisterValidatorsFromAssemblyContaining<Program>();
        c.ImplicitlyValidateChildProperties = true;
        c.DisableDataAnnotationsValidation = true;
    });

builder.Services.AddRouting(o => { o.LowercaseUrls = true; });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(RouteConstants.PublicApi(1),
        new OpenApiInfo {Title = "Public Molly's Movies API", Version = "v1"});
    c.CustomSchemaIds(t => t.DtoSafeFriendlyId());
    c.CustomOperationIds(ad => ad.GetOperationId());
    c.DocInclusionPredicate((name, api) => api.IsApi(name));
});

// Scraper
builder.Services
    .AddTransient<IScrapeRepository, ScrapeRepository>()
    .AddTransient<IScrapeService, ScrapeService>()
    .AddSingleton<IScrapeMapper, ScrapeMapper>()
    .AddTransient<IScraperClient, MassTransitScraperClient>();

// Movie Images
builder.Services.AddOptions<MovieImageOptions>()
    .BindConfiguration("MovieImages")
    .ValidateFluentValidator().ValidateOnStart();
builder.Services
    .AddSingleton<IMovieImageFileProviderFactory, MovieImageFileProviderFactory>();
    
// Movies
builder.Services
    .AddTransient<IMovieRepository, MovieRepository>()
    .AddTransient<IMovieService, MovieService>()
    .AddTransient<IMovieDownloadService, MovieDownloadService>()
    .AddSingleton<IMovieMapper, MovieMapper>();

// Transmission
builder.Services.AddOptions<TransmissionOptions>()
    .BindConfiguration("Transmission")
    .ValidateFluentValidator().ValidateOnStart();

builder.Services.AddTransmissionRpcClient();

builder.Services
    .AddTransient<ITransmissionDownloadService, TransmissionDownloadService>()
    .AddTransient<ITorrentService, TorrentService>()
    .AddTransient<ITransmissionService, TransmissionService>()
    .AddTransient<IMagnetUriService, MagnetUriService>();

// Common
builder.Services
    .AddSingleton<ISystemClock, SystemClock>()
    .AddSingleton<IFileSystem, FileSystem>();

var app = builder.Build();

app.MapProbes();

app.UseSwagger();
app.UseSwaggerUI(c => c.PublicApi(1));

app.MapControllers();

app.UseStaticMovieImages();

app.Run();

/// <summary>
///     Redefined implicit Program class as public for e2e testing.
/// </summary>
public partial class Program
{
}