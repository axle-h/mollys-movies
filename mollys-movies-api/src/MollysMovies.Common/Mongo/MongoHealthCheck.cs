using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace MollysMovies.Common.Mongo;

public class MongoHealthCheck : IHealthCheck
{
    private readonly IMongoDatabase _database;
    private readonly IMongoInitService _initService;

    public MongoHealthCheck(IMongoDatabase database, IMongoInitService initService)
    {
        _database = database;
        _initService = initService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.ListCollectionNamesAsync(null, cancellationToken);
            await _initService.WaitAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(exception: e);
        }
    }
}