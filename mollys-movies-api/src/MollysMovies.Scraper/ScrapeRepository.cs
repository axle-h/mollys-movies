using System;
using System.Threading;
using System.Threading.Tasks;
using MollysMovies.Common.Scraper;
using MongoDB.Driver;

namespace MollysMovies.Scraper;

public interface IScrapeRepository
{
    Task<Scrape?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task ReplaceAsync(Scrape scrape, CancellationToken cancellationToken = default);
}

public class ScrapeRepository : IScrapeRepository
{
    private readonly IMongoDatabase _database;

    public ScrapeRepository(IMongoDatabase database)
    {
        _database = database;
    }

    private IMongoCollection<Scrape> Collection => _database.GetCollection<Scrape>(Scrape.CollectionName);

    public async Task<Scrape?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        await Collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task ReplaceAsync(Scrape scrape, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(scrape.Id))
        {
            throw new ArgumentException("scrape id is required", nameof(scrape));
        }
        await Collection.ReplaceOneAsync(x => x.Id == scrape.Id, scrape, new ReplaceOptions {IsUpsert = false}, cancellationToken);
    }
}