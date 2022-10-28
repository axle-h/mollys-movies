using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MollysMovies.Common.Mongo;
using MollysMovies.Common.Movies;
using MollysMovies.FakeData;
using MollysMovies.Scraper.E2e.Fixtures;
using MollysMovies.Scraper.Movies;
using MongoDB.Driver;
using Xunit;

namespace MollysMovies.Scraper.E2e.Movies;

public class MongoTestFixture<TEntity, TRepository> : IDisposable
    where TRepository : class
{
    private readonly ServiceProvider _services;

    public MongoTestFixture()
    {
        var collectionName = typeof(TEntity)
            .GetField("CollectionName", BindingFlags.Static | BindingFlags.Public)
            ?.GetValue(null) as string ?? throw new Exception($"no CollectionName field found on entity {typeof(TEntity)}");
        
        _services = new ServiceCollection()
            .AddMongo(new MongoUrl(TestEnvironment.MongoUrl(Fake.NicelyDatedString(collectionName))))
            .AddLogging(o => o.AddConsole())
            .Configure<LoggerFilterOptions>(o => o.MinLevel = LogLevel.Debug)
            .AddSingleton<TRepository>()
            .BuildServiceProvider();
        
        Collection = _services
            .GetRequiredService<IMongoDatabase>()
            .GetCollection<TEntity>(collectionName);
        
        Subject = _services.GetRequiredService<TRepository>();
    }
    
    public TRepository Subject { get; }
    
    public IMongoCollection<TEntity> Collection { get; }
    
    public void Dispose()
    {
        _services.Dispose();
    }
}

public class MovieRepositoryTests : IClassFixture<MongoTestFixture<Movie, MovieRepository>>
{
    private readonly MongoTestFixture<Movie, MovieRepository> _fixture;

    public MovieRepositoryTests(MongoTestFixture<Movie, MovieRepository> fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Creating_new_movie_from_remote()
    {
        var meta = Fake.MovieMeta.Generate();
        var torrents = Fake.Torrent.Generate(1);
        var imdb = Fake.Faker.ImdbCode();
        
        var observed = await _fixture.Subject.UpsertFromRemoteAsync(imdb, meta, torrents);
        var movie = await _fixture.Collection.Find(x => x.ImdbCode == imdb).SingleAsync();

        using var scope = new AssertionScope();
        observed.Should().BeEquivalentTo(movie);
        movie.Should().BeEquivalentTo(new Movie
        {
            ImdbCode = imdb,
            Meta = meta,
            Torrents = torrents
        }, o => o.DatesToNearestSecond());
    }
    
    [Fact]
    public async Task Updating_local_movie_from_remote()
    {
        var movie = Fake.Movie.With(m =>
        {
            m.Download = null;
            m.Torrents = new List<Torrent>();
        }).Generate();
        var meta = Fake.MovieMeta.Generate();
        var torrents = Fake.Torrent.Generate(1);

        await _fixture.Collection.InsertOneAsync(movie);
        
        var observed = await _fixture.Subject.UpsertFromRemoteAsync(movie.ImdbCode, meta, torrents);
        var updatedMovie = await _fixture.Collection.Find(x => x.ImdbCode == movie.ImdbCode).SingleAsync();

        using var scope = new AssertionScope();
        observed.Should().BeEquivalentTo(updatedMovie);
        updatedMovie.Should().BeEquivalentTo(new Movie
        {
            ImdbCode = movie.ImdbCode,
            Meta = meta, // prefer all meta from remote over local
            Torrents = torrents,
            LocalSource = movie.LocalSource
        }, o => o.DatesToNearestSecond());
    }
    
    [Fact]
    public async Task Creating_new_movie_from_local()
    {
        var meta = Fake.MovieMeta.Generate();
        var source = Fake.LocalMovieSource.Generate();
        var imdb = Fake.Faker.ImdbCode();
        
        var observed = await _fixture.Subject.UpsertFromLocalAsync(imdb, meta, source);
        var movie = await _fixture.Collection.Find(x => x.ImdbCode == imdb).SingleAsync();

        using var scope = new AssertionScope();
        observed.Should().BeEquivalentTo(movie);
        movie.Should().BeEquivalentTo(new Movie
        {
            ImdbCode = imdb,
            LocalSource = source,
            Meta = meta,
            Torrents = new List<Torrent>()
        }, o => o.DatesToNearestSecond());
    }

    [Fact]
    public async Task Updating_remote_movie_with_local()
    {
        var movie = Fake.Movie.With(m =>
        {
            m.Download = null;
            m.LocalSource = null;
        }).Generate();
        var meta = Fake.MovieMeta.Generate();
        var source = Fake.LocalMovieSource.Generate();
        await _fixture.Collection.InsertOneAsync(movie);
        
        var observed = await _fixture.Subject.UpsertFromLocalAsync(movie.ImdbCode, meta, source);
        var updatedMovie = await _fixture.Collection.Find(x => x.ImdbCode == movie.ImdbCode).SingleAsync();

        using var scope = new AssertionScope();
        observed.Should().BeEquivalentTo(updatedMovie);
        updatedMovie.Should().BeEquivalentTo(new Movie
        {
            ImdbCode = movie.ImdbCode,
            LocalSource = source,
            Meta = movie.Meta, // prefer meta from remote
            Torrents = movie.Torrents
        }, o => o.DatesToNearestSecond());
    }

}