using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MakeMovies.Api.Library.Jellyfin;

public class JellyfinHealthCheck(JellyfinClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        try
        {
            await client.ListAllAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(exception: e);
        }
    }
}