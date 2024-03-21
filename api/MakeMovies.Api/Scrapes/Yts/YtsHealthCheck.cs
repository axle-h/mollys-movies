using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MakeMovies.Api.Scrapes.Yts;

public class YtsHealthCheck(IYtsClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        try
        {
            await client.ListMoviesAsync(1, 1, cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(exception: e);
        }
    }
}