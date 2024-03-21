using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MakeMovies.Api.Meta.Tmdb;

public class TmdbHealthCheck(TmdbClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        try
        {
            await client.Three.Configuration.GetAsync(cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(exception: e);
        }
    }
}