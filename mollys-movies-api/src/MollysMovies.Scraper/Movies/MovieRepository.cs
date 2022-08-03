using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MollysMovies.Common.Movies;
using MongoDB.Driver;

namespace MollysMovies.Scraper.Movies;

public interface IMovieRepository
{
    Task<bool> LocalMovieExistsAsync(string imdbCode, CancellationToken cancellationToken = default);

    Task<Movie> AddDownloadStatus(string imdbCode, MovieDownloadStatus downloadStatus,
        CancellationToken cancellationToken = default);

    Task<Movie> UpsertFromRemoteAsync(string imdbCode, MovieMeta meta,
        IEnumerable<Torrent> torrents, CancellationToken cancellationToken = default);

    Task<Movie> UpsertFromLocalAsync(string imdbCode, MovieMeta meta, LocalMovieSource localSource,
        CancellationToken cancellationToken = default);

    Task<DateTime?> GetLatestLocalMovieBySourceAsync(string source, CancellationToken cancellationToken = default);

    Task<DateTime?> GetLatestMovieBySourceAsync(string source, CancellationToken cancellationToken = default);

    Task UpdateMovieImageAsync(string imdbCode, string imageFilename, CancellationToken cancellationToken = default);

    Task<Movie?> GetMovieByDownloadExternalIdAsync(string externalId, CancellationToken cancellationToken);
}

public class MovieRepository : IMovieRepository
{
    private readonly IMongoDatabase _database;

    public MovieRepository(IMongoDatabase database)
    {
        _database = database;
    }

    private IMongoCollection<Movie> Collection => _database.GetCollection<Movie>(Movie.CollectionName);

    private static FilterDefinitionBuilder<Movie> Filters => Builders<Movie>.Filter;

    public async Task<bool> LocalMovieExistsAsync(string imdbCode, CancellationToken cancellationToken = default) =>
        await Collection.Find(x => x.ImdbCode == imdbCode && x.LocalSource != null).AnyAsync(cancellationToken);

    public async Task<Movie> AddDownloadStatus(string imdbCode, MovieDownloadStatus downloadStatus,
        CancellationToken cancellationToken = default) =>
        await UpdateByImdbCode(imdbCode, Builders<Movie>.Update.AddToSet(x => x.Download!.Statuses, downloadStatus),
            false, cancellationToken);

    public async Task<Movie> UpsertFromRemoteAsync(string imdbCode, MovieMeta meta,
        IEnumerable<Torrent> torrents, CancellationToken cancellationToken = default) =>
        await UpdateByImdbCode(imdbCode,
            Builders<Movie>.Update
                .Set(x => x.Meta, meta)
                .AddToSetEach(x => x.Torrents, torrents),
            true, cancellationToken);

    public async Task<Movie> UpsertFromLocalAsync(string imdbCode, MovieMeta meta, LocalMovieSource localSource,
        CancellationToken cancellationToken = default) =>
        await UpdateByImdbCode(imdbCode,
            Builders<Movie>.Update
                .SetOnInsert(x => x.Meta, meta)
                .Set(x => x.LocalSource, localSource),
            true, cancellationToken);

    public async Task<DateTime?> GetLatestLocalMovieBySourceAsync(string source, CancellationToken cancellationToken = default)
    {
        var movie = await Collection
            .Find(x => x.LocalSource != null && x.LocalSource.Source == source)
            .SortByDescending(x => x.LocalSource!.DateCreated)
            .Limit(1)
            .FirstOrDefaultAsync(cancellationToken);
        return movie?.LocalSource?.DateCreated;
    }

    public async Task<DateTime?> GetLatestMovieBySourceAsync(string source, CancellationToken cancellationToken = default)
    {
        var movie = await Collection
            .Find(x => x.Meta != null && x.Meta.Source == source)
            .SortByDescending(x => x.Meta!.DateCreated)
            .Limit(1)
            .FirstOrDefaultAsync(cancellationToken);
        return movie?.Meta!.DateCreated;
    }

    public async Task UpdateMovieImageAsync(string imdbCode, string imageFilename, CancellationToken cancellationToken = default)
    {
        await UpdateByImdbCode(imdbCode,
            Builders<Movie>.Update.Set(x => x.Meta!.ImageFilename, imageFilename),
            false, cancellationToken);
    }
    
    public async Task<Movie?> GetMovieByDownloadExternalIdAsync(string externalId, CancellationToken cancellationToken) =>
        await Collection.Find(x => x.Download != null && x.Download.ExternalId == externalId)
            .FirstOrDefaultAsync(cancellationToken);

    private async Task<Movie> UpdateByImdbCode(string imdbCode, UpdateDefinition<Movie> update,
        bool isUpsert,
        CancellationToken cancellationToken)
    {
        var filter = Filters.Eq(x => x.ImdbCode, imdbCode);
        return await Collection.FindOneAndUpdateAsync(filter, update,
            new FindOneAndUpdateOptions<Movie>
                {IsUpsert = isUpsert, ReturnDocument = ReturnDocument.After},
            cancellationToken);
    }
}