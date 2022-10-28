using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MollysMovies.Scraper.Health;

public class ScraperHealthCheck : IHealthCheck
{
    private readonly List<IScraper> _scrapers;
    private readonly IMemoryCache _cache;

    public ScraperHealthCheck(IEnumerable<IScraper> scrapers, IMemoryCache cache)
    {
        _cache = cache;
        _scrapers = scrapers.ToList();
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var scraper = _scrapers.FirstOrDefault(x => x.Source == context.Registration.Name)
                      ?? throw new Exception($"cannot find scraper with name {context.Registration.Name}");
        
        // we need to be careful sending too many requests to these external services
        return await _cache.GetOrCreateAsync($"health-{scraper.Source}", async entry =>
        {
            try
            {
                await scraper.HealthCheckAsync(cancellationToken);
                // if we get a good result then cache it for 60 mins
                entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
                return HealthCheckResult.Healthy();
            }
            catch (Exception e)
            {
                // otherwise cache it for a minute
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                return HealthCheckResult.Unhealthy(exception: e);
            }
        });
    }
}