using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MollysMovies.Common;
using MollysMovies.Common.Mongo;
using MollysMovies.Common.Movies;
using MollysMovies.Common.Scraper;
using MollysMovies.FakeData;
using MollysMovies.ScraperClient;
using MongoDB.Driver;
using WireMock.Server;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace MollysMovies.Scraper.E2e;

public class MollysMoviesScraperFixture : WebApplicationFactory<Program>
{
    private readonly string _testRunId;

    public MollysMoviesScraperFixture(string name)
    {
        _testRunId = Fake.NicelyDatedString($"scraper-{name}");
    }

    static MollysMoviesScraperFixture()
    {
        EndpointConvention.Map<StartScrape>(new Uri("queue:start-scrape"));
    }

    public MockFileSystem FileSystem { get; } = new();

    public IMongoDatabase Database => Service<IMongoDatabase>();

    public IMongoCollection<Movie> Movies => Database.GetCollection<Movie>(Movie.CollectionName);

    public IMongoCollection<Scrape> Scrapes => Database.GetCollection<Scrape>(Scrape.CollectionName);

    public IPublishEndpoint PublishEndpoint => Service<IPublishEndpoint>();

    public WireMockServer WireMock { get; } = WireMockServer.Start();
    
    public RabbitMqTestHarness.Builder RabbitMq() =>
        new(Services.GetRequiredService<IConfiguration>().GetConnectionUrl("rabbitmq").ToString(), _testRunId);

    private T Service<T>() where T : notnull => Server.Services.GetRequiredService<T>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        FileSystem.AddDirectory("/movie-images");
        FileSystem.AddDirectory("/movie-library");
        FileSystem.AddDirectory("/var/downloads");

        builder.UseEnvironment("e2e")
            .ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:mongo"] = $"mongodb://{GetConfiguredHost("mongo")}:27017/{_testRunId}",
                ["ConnectionStrings:rabbitmq"] = $"rabbitmq://user:password@{GetConfiguredHost("rabbitmq")}:5672/{_testRunId}",
                ["ConnectionStrings:plex"] = $"{WireMock.Urls[0]}/plex/",
                ["ConnectionStrings:yts"] = $"{WireMock.Urls[0]}/yts/",
                ["ConnectionStrings:transmission"] = $"{WireMock.Urls[0]}/transmission/"
            }))
            .ConfigureServices(services =>
            {
                services.Replace(new ServiceDescriptor(typeof(IFileSystem), FileSystem));
                services.Configure<ScraperOptions>(o =>
                {
                    o.ImagePath = "/movie-images";
                    o.MovieLibraryPath = "/movie-library";
                    o.DownloadsPath = "/var/downloads";
                    o.ImageScraperPageSize = 50;
                    o.RemoteScrapeDelay = TimeSpan.Zero;
                    o.LocalUpdateMovieDelay = TimeSpan.Zero;
                    o.Yts = new YtsOptions
                    {
                        Limit = 50,
                        RetryDelay = TimeSpan.Zero
                    };
                    o.Plex = new PlexOptions
                    {
                        Token = "some-plex-token"
                    };
                });
                services.Configure<MongoInitOptions>(o => o.Seed = o.Index = true);
            });
    }

    private async Task EnsureRabbitMqVirtualHostAsync()
    {
        using var handler = new HttpClientHandler { Credentials = new NetworkCredential { UserName = "user", Password = "password" } };
        using var client = new HttpClient(handler);
        var content = new StringContent("", Encoding.UTF8, "application/json");
        var result = await client.PutAsync($"http://{GetConfiguredHost("rabbitmq")}:15672/api/vhosts/{_testRunId}", content);
        result.EnsureSuccessStatusCode();
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        EnsureRabbitMqVirtualHostAsync().Wait();
        var host = base.CreateHost(builder);

        async Task StartedAndHealthyAsync()
        {
            var server = (TestServer) host.Services.GetRequiredService<IServer>();
            var result = await server.CreateClient().GetAsync("/health/live");
            result.EnsureSuccessStatusCode();
        }

        StartedAndHealthyAsync().Wait();
        return host;
    }

    public async Task AddMoviesAsync(params Movie[] movies)
    {
        await Movies.InsertManyAsync(movies);
    }

    public override async ValueTask DisposeAsync()
    {
        await Service<IMongoClient>().DropDatabaseAsync(_testRunId);
        await base.DisposeAsync();
        WireMock.Stop();
    }
    
    private static string GetConfiguredHost(string name) =>
        Environment.GetEnvironmentVariable($"{name.ToUpper()}_HOST") ?? "localhost";
}