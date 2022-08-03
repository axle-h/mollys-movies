using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MollysMovies.Scraper.Health;

public class ScraperHealthCheck : IHealthCheck
{
    private readonly List<IScraper> _scrapers;

    public ScraperHealthCheck(IEnumerable<IScraper> scrapers)
    {
        _scrapers = scrapers.ToList();
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var scraper = _scrapers.FirstOrDefault(x => x.Source == context.Registration.Name)
                      ?? throw new Exception($"cannot find scraper with name {context.Registration.Name}");

        try
        {
            await scraper.HealthCheckAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(exception: e);
        }
    }
}