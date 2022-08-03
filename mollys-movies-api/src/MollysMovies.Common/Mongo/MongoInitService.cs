using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MollysMovies.Common.Movies;
using MollysMovies.Common.Scraper;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MollysMovies.Common.Mongo;

public interface IMongoInitService : IHostedService
{
    Task WaitAsync(CancellationToken cancellationToken = default);
}

public class MongoInitService : BackgroundService, IMongoInitService
{
    private readonly TaskCompletionSource _complete = new();
    private readonly IMongoDatabase _database;
    private readonly MongoInitOptions _options;

    public MongoInitService(IOptions<MongoInitOptions> options, IMongoDatabase database)
    {
        _database = database;
        _options = options.Value;
    }

    private IMongoCollection<Movie> Movies => _database.GetCollection<Movie>(Movie.CollectionName);

    private IMongoCollection<Scrape> Scrapes => _database.GetCollection<Scrape>(Scrape.CollectionName);

    public Task WaitAsync(CancellationToken cancellationToken = default) =>
        _complete.Task.WaitAsync(cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (_options.Index)
            {
                await CreateMovieIndexes(stoppingToken);
            }

            if (_options.Seed)
            {
                await Movies.InsertManyAsync(TestSeedData.Movies, cancellationToken: stoppingToken);
                await Scrapes.InsertManyAsync(TestSeedData.Scrapes, cancellationToken: stoppingToken);
            }

            _complete.SetResult();
        }
        catch (Exception e)
        {
            _complete.SetException(e);
        }
    }

    private async Task CreateMovieIndexes(CancellationToken cancellationToken)
    {
        var indexes = new List<CreateIndexModel<Movie>>
        {
            new(Builders<Movie>.IndexKeys.Text(x => x.Meta!.Title),
                new CreateIndexOptions
                {
                    Weights = new BsonDocument {{nameof(MovieMeta.Title), 2}, {nameof(MovieMeta.Description), 1}}
                }),
            new(Builders<Movie>.IndexKeys.Ascending(x => x.Meta!.Rating)),
            new(Builders<Movie>.IndexKeys
                .Ascending(x => x.Meta!.Source)
                .Ascending(x => x.Meta!.DateCreated)),
            new(Builders<Movie>.IndexKeys.Ascending(x => x.Meta!.Year)),
            new(Builders<Movie>.IndexKeys.Ascending(x => x.Meta!.Genres)),
            new(Builders<Movie>.IndexKeys.Ascending(x => x.Download!.ExternalId),
                new CreateIndexOptions {Sparse = true})
        };
        await Movies.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}