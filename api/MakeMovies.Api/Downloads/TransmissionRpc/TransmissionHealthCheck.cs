using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MakeMovies.Api.Downloads.TransmissionRpc;

public class TransmissionHealthCheck(ITransmissionRpcClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.GetAllTorrentsAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(exception: e);
        }
    }
}