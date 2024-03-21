namespace MakeMovies.Api.Downloads;



public interface IDownloadRepository
{
    Task AddAsync(Download download, CancellationToken cancellationToken = default);

    Task<Download?> GetByMovieIdAsync(string movieId, CancellationToken cancellationToken = default);

    Task<List<Download>> AllActiveAsync(CancellationToken cancellationToken = default);
    
    Task<PaginatedData<Download>> ListAsync(
        PaginatedQuery<DownloadField> query,
        CancellationToken cancellationToken = default);

    Task<Download?> GetAsync(string id, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Download download, CancellationToken cancellationToken = default);
}

public class DownloadRepository(Db db) : IDownloadRepository
{
    public async Task AddAsync(Download download, CancellationToken cancellationToken = default)
    {
        await db.Downloads.UpsertAsync(download.Id, download, cancellationToken);
    }

    public Task<Download?> GetByMovieIdAsync(string movieId, CancellationToken cancellationToken = default) =>
        Task.FromResult(db.Downloads.AsEnumerable().FirstOrDefault(x => x.MovieId == movieId));

    public Task<List<Download>> AllActiveAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(db.Downloads.AsEnumerable()
            .Where(d => !d.Complete)
            .ToList());

    public Task<PaginatedData<Download>> ListAsync(
        PaginatedQuery<DownloadField> query,
        CancellationToken cancellationToken = default)
    {
        var scrapes = db.Downloads.AsQueryable();
        var pageData = scrapes
            .OrderByDescending(x => x.StartDate)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToList();
        return Task.FromResult(new PaginatedData<Download>(query.Page, query.Limit, db.Scrapes.Count, pageData));
    }

    public Task<Download?> GetAsync(string id, CancellationToken cancellationToken = default) =>
        Task.FromResult(db.Downloads.Get(id));

    public async Task UpdateAsync(Download download, CancellationToken cancellationToken = default)
    {
        await db.Downloads.UpsertAsync(download.Id, download, cancellationToken);
    }
}