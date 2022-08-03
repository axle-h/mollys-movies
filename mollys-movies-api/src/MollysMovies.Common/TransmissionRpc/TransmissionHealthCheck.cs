using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MollysMovies.Common.TransmissionRpc;

public class TransmissionHealthCheck : IHealthCheck
{
    private readonly ITransmissionRpcClient _client;

    public TransmissionHealthCheck(ITransmissionRpcClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.GetAllTorrentsAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(exception: e);
        }
    }
}