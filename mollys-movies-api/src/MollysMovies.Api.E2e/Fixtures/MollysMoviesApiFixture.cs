using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MollysMovies.Api.MovieImages;
using MollysMovies.Api.Transmission;
using MollysMovies.Client.Api;
using MollysMovies.Common;
using MollysMovies.Common.Mongo;
using MollysMovies.Common.Movies;
using MollysMovies.Common.Scraper;
using MollysMovies.FakeData;
using MongoDB.Driver;
using WireMock.Server;
using Xunit;

namespace MollysMovies.Api.E2e.Fixtures;

[Collection(MollysMoviesApiFixtureDefinition.Name)]
public abstract class MollysMoviesApiTests
{
    protected MollysMoviesApiTests(MollysMoviesApiFixture fixture)
    {
        Fixture = fixture;
        Fixture.WireMock.Reset();
    }

    protected MollysMoviesApiFixture Fixture { get; }
}

[CollectionDefinition(Name)]
public class MollysMoviesApiFixtureDefinition : ICollectionFixture<MollysMoviesApiFixture>
{
    public const string Name = "Molly's Movies API";
}

public sealed class MollysMoviesApiFixture : WebApplicationFactory<Program>
{
    private readonly string _testRunId = Fake.NicelyDatedString("api");

    public IMongoDatabase Database => Service<IMongoDatabase>();

    public IMongoCollection<Movie> Movies => Database.GetCollection<Movie>(Movie.CollectionName);

    public IMongoCollection<Scrape> Scrapes => Database.GetCollection<Scrape>(Scrape.CollectionName);

    public WireMockServer WireMock { get; } = WireMockServer.Start();

    public MockFileSystem FileSystem { get; } = new();

    public GenreApi GenreApi => new(CreateClient());

    public MoviesApi MoviesApi => new(CreateClient());

    public ScrapeApi ScrapeApi => new(CreateClient());

    public TorrentApi TorrentApi => new(CreateClient());

    public TransmissionApi TransmissionApi => new(CreateClient());

    private T Service<T>() where T : notnull => Server.Services.GetRequiredService<T>();

    public RabbitMqTestHarness.Builder RabbitMq(string name) =>
        new(Services.GetRequiredService<IConfiguration>().GetConnectionUrl("rabbitmq").ToString(), name);
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        FileSystem.AddDirectory("/movie-images");
        FileSystem.AddDirectory("/var/downloads");

        builder.UseEnvironment("e2e")
            .ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:mongo"] = TestEnvironment.MongoUrl(_testRunId),
                ["ConnectionStrings:rabbitmq"] = TestEnvironment.RabbitMqUrl(_testRunId),
                ["ConnectionStrings:transmission"] = $"{WireMock.Urls[0]}/transmission/",
                // the images aren't actually stored in this mock filesystem, this is just to pass validation... yawn
                ["MovieImages:Path"] = "/movie-images"
            }))
            .ConfigureServices(services =>
            {
                // replace very difficult to stub file provider with a dummy one
                services.Replace(new ServiceDescriptor(typeof(IMovieImageFileProviderFactory), new DummyMovieImageFileProviderFactory()));
                services.Replace(new ServiceDescriptor(typeof(IFileSystem), FileSystem));

                services.Configure<TransmissionOptions>(o =>
                {
                    o.Trackers = new List<string> {"udp://tracker1:1337/announce", "udp://tracker2:1337/announce"};
                });
                services.Configure<MongoInitOptions>(o => o.Seed = o.Index = true);
            });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        TestEnvironment.EnsureRabbitMqVirtualHostAsync(_testRunId).Wait();
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
    
    private class DummyMovieImageFileProviderFactory : IMovieImageFileProviderFactory
    {
        private static readonly Assembly FakeAssembly = typeof(Fake).Assembly;
        
        public IFileProvider Build() => new EmbeddedFileProvider(FakeAssembly, $"{FakeAssembly.GetName().Name}.MovieImages");
    }
}