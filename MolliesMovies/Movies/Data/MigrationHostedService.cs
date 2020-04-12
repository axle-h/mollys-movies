using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MolliesMovies.Common.Data;

namespace MolliesMovies.Movies.Data
{
    public class MigrationHostedService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<MigrationHostedService> _logger;

        public MigrationHostedService(IServiceProvider provider, IHostApplicationLifetime lifetime, ILogger<MigrationHostedService> logger)
        {
            _provider = provider;
            _lifetime = lifetime;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Migrating database");

            try
            {
                using var scope = _provider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<MolliesMoviesContext>();
                await context.Database.MigrateAsync(stoppingToken);
                _logger.LogInformation("done");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to migrate database");
                throw;
            }
            finally
            {
                _lifetime.StopApplication();
            }
        }
    }
}