using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MollysMovies.Common.Scraper;
using MongoDB.Driver;

namespace MollysMovies.Api.Scraper;

public interface IScrapeRepository
{
    Task<List<Scrape>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Scrape> InsertScrapeAsync(DateTime startDate, CancellationToken cancellationToken = default);
}

public class ScrapeRepository : IScrapeRepository
{
    private readonly IMongoDatabase _database;

    public ScrapeRepository(IMongoDatabase database)
    {
        _database = database;
    }

    private IMongoCollection<Scrape> Collection => _database.GetCollection<Scrape>(Scrape.CollectionName);

    private static FilterDefinitionBuilder<Scrape> Filters => Builders<Scrape>.Filter;

    public async Task<List<Scrape>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Collection.Find(Filters.Empty).SortByDescending(x => x.StartDate).ToListAsync(cancellationToken);

    public async Task<Scrape> InsertScrapeAsync(DateTime startDate, CancellationToken cancellationToken = default)
    {
        var scrape = new Scrape {StartDate = startDate};
        await Collection.InsertOneAsync(scrape, null, cancellationToken);
        return scrape;
    }
}