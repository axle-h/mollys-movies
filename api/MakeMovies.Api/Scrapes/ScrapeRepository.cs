namespace MakeMovies.Api.Scrapes;

public interface IScrapeRepository
{
    Task<Scrape> NewAsync(CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Scrape scrape, CancellationToken cancellationToken = default);

    Task<PaginatedData<Scrape>> ListAsync(PaginatedQuery<ScrapeField> query, CancellationToken cancellationToken = default);
}

public class JsonScrapeRepository(Db db) : IScrapeRepository
{
    public async Task<Scrape> NewAsync(CancellationToken cancellationToken = default)
    {
        var scrape = new Scrape(Guid.NewGuid().ToString(), DateTime.UtcNow);
        await db.Scrapes.UpsertAsync(scrape.Id, scrape, cancellationToken);
        return scrape;
    }

    public async Task UpdateAsync(Scrape scrape, CancellationToken cancellationToken = default)
    {
        await db.Scrapes.UpsertAsync(scrape.Id, scrape, cancellationToken);
    }

    public Task<PaginatedData<Scrape>> ListAsync(PaginatedQuery<ScrapeField> query, CancellationToken cancellationToken = default)
    {
        var scrapes = db.Scrapes.AsQueryable();
        var pageData = scrapes
            .OrderByDescending(x => x.StartDate)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToList();
        return Task.FromResult(new PaginatedData<Scrape>(query.Page, query.Limit, db.Scrapes.Count, pageData));
    }
}