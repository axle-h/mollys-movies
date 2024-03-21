using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MakeMovies.Api.Meta.Omdb;

public class OmdbHealthCheck(OmdbClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        try
        {
            await client.GetByImdbCodeAsync("tt0816692", cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(exception: e);
        }
    }
}