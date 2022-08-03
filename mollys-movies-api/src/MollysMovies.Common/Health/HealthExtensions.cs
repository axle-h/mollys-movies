using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MollysMovies.Common.Health;

public static class HealthExtensions
{
    private const string ReadyTag = "ready";

    public static IHealthChecksBuilder AddProbe<T>(
        this IHealthChecksBuilder builder,
        string name,
        HealthStatus? failureStatus = null,
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