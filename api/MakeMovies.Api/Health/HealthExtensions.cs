using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MakeMovies.Api.Health;

public static class HealthExtensions
{
    private const string ReadyTag = "ready";

    public static IHealthChecksBuilder AddProbe<T>(
        this IHealthChecksBuilder builder,
        string name,
        HealthStatus failureStatus = HealthStatus.Degraded,
        TimeSpan? timeout = null) where T : class, IHealthCheck =>
        builder.AddCheck<T>(name, failureStatus, new[] {ReadyTag}, timeout);

    public static void MapProbes(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions {Predicate = _ => false});
    }
}