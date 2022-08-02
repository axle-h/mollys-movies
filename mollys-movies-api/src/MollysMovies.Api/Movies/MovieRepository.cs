using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MollysMovies.Api.Common;
using MollysMovies.Api.Movies.Requests;
using MollysMovies.Common.Movies;
using MongoDB.Driver;

namespace MollysMovies.Api.Movies;

public interface IMovieRepository
{
    Task<Movie?> GetByImdbCodeAsync(string imdbCode, CancellationToken cancellationToken = default);

    Task<Movie?> GetByExternalDownloadIdAsync(string externalDownloadId,
        CancellationToken cancellationToken = default);

    Task<PaginatedData<Movie>> SearchAsync(PaginatedMovieQuery query, CancellationToken cancellationToken = default);

    Task<Movie> ReplaceDownload(string imdbCode, MovieDownload download,
        CancellationToken cancellationToken = default);

    Task<Movie> AddDownloadStatus(string imdbCode, MovieDownloadStatus downloadStatus,
        CancellationToken cancellationToken = default);

    Task<HashSet<string>> GetAllGenresAsync(CancellationToken cancellationToken = default);
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

    private static SortDefinitionBuilder<Movie> Sort => Builders<Movie>.Sort;

    public async Task<Movie?> GetByImdbCodeAsync(string imdbCode, CancellationToken cancellationToken = default)
    {
        var cursor = await Collection.FindAsync(x => x.ImdbCode == imdbCode, cancellationToken: cancellationToken);
        return await cursor.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Movie?> GetByExternalDownloadIdAsync(string externalDownloadId,
        CancellationToken cancellationToken = default)
    {
        var cursor = await Collection.FindAsync(x => x.Download!.ExternalId == externalDownloadId,
            cancellationToken: cancellationToken);
        return await cursor.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginatedData<Movie>> SearchAsync(PaginatedMovieQuery query, CancellationToken cancellationToken = default)
    {
        var filters = new List<FilterDefinition<Movie>>();
        if (query.Text is not null)
        {
            filters.Add(Filters.Text(query.Text, new TextSearchOptions
            {
                Language = "en", CaseSensitive = false, DiacriticSensitive = false
            }));
        }

        if (query.Quality is not null)
        {
            filters.Add(Filters.ElemMatch(x => x.Torrents, x => x.Quality == query.Quality));
        }

        if (query.Language is not null)
        {
            filters.Add(Filters.Eq(x => x.Meta!.Language, query.Language));
        }

        switch (query.Downloaded)
        {
            case true:
                filters.Add(Filters.Exists(x => x.LocalSource));
                filters.Add(Filters.Not(Filters.Eq(x => x.LocalSource, null)));
                break;

            case false:
                filters.Add(Filters.Or(
                    Filters.Not(Filters.Exists(x => x.LocalSource)),
                    Filters.Eq(x => x.LocalSource, null)
                ));
                break;
        }

        switch (query.HasDownload)
        {
            case true:
                filters.Add(Filters.Exists(x => x.Download));
                break;

            case false:
                filters.Add(Filters.Not(Filters.Exists(x => x.Download)));
                break;
        }

        if (!string.IsNullOrEmpty(query.Genre))
        {
            filters.Add(Filters.AnyEq(x => x.Meta!.Genres, query.Genre));
        }

        if (query.YearFrom.HasValue)
        {
            filters.Add(Filters.Gte(x => x.Meta!.Year, query.YearFrom));
        }

        if (query.YearTo.HasValue)
        {
            filters.Add(Filters.Lte(x => x.Meta!.Year, query.YearTo));
        }

        if (query.RatingFrom.HasValue)
        {
            filters.Add(Filters.Gte(x => x.Meta!.Rating, query.RatingFrom));
        }

        if (query.RatingTo.HasValue)
        {
            filters.Add(Filters.Lte(x => x.Meta!.Rating, query.RatingTo));
        }

        var filter = filters.Any() ? Filters.And(filters) : Filters.Empty;
        var sorts = query.OrderBy
                        ?.Select(x => x.Descending ? Sort.Descending(x.Property) : Sort.Ascending(x.Property))
                    ?? Array.Empty<SortDefinition<Movie>>();


        var countFacet = AggregateFacet.Create("count",
            PipelineDefinition<Movie, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<Movie>()
            }));

        var dataFacet = AggregateFacet.Create("data",
            PipelineDefinition<Movie, Movie>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Sort(Sort.Combine(sorts)),
                PipelineStageDefinitionBuilder.Skip<Movie>((query.Page - 1) * query.Limit),
                PipelineStageDefinitionBuilder.Limit<Movie>(query.Limit)
            }));

        var aggregation = await Collection.Aggregate()
            .Match(filter)
            .Facet(countFacet, dataFacet)
            .ToListAsync(cancellationToken);

        var count = aggregation.First()
            .Facets.First(x => x.Name == "count")
            .Output<AggregateCountResult>()
            ?.FirstOrDefault()
            ?.Count ?? 0;

        var data = aggregation.First()
            .Facets.First(x => x.Name == "data")
            .Output<Movie>()
            .ToList();

        return new PaginatedData<Movie>
        {
            Page = query.Page,
            Limit = query.Limit,
            Count = count,
            Data = data
        };
    }

    public async Task<Movie> ReplaceDownload(string imdbCode, MovieDownload download,
        CancellationToken cancellationToken = default) =>
        await UpdateByImdbCode(imdbCode, Builders<Movie>.Update.Set(x => x.Download, download),
            false, cancellationToken);

    public async Task<Movie> AddDownloadStatus(string imdbCode, MovieDownloadStatus downloadStatus,
        CancellationToken cancellationToken = default) =>
        await UpdateByImdbCode(imdbCode, Builders<Movie>.Update.AddToSet(x => x.Download!.Statuses, downloadStatus),
            false, cancellationToken);

    public async Task<HashSet<string>> GetAllGenresAsync(CancellationToken cancellationToken = default)
    {
        var cursor = await Collection.DistinctAsync(
            new StringFieldDefinition<Movie, string>("Meta.Genres"),
            Filters.SizeGt(x => x.Meta!.Genres, 0),
            cancellationToken: cancellationToken);
        var genres = await cursor.ToListAsync(cancellationToken);
        return genres.ToHashSet();
    }

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