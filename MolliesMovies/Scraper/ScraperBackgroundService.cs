using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MolliesMovies.Scraper
{
    public interface IScraperBackgroundService
    {
        void AddScrapeAllJob(int id);
        void AddScrapeForLocalMovieJob(int movieId);
    }

    public class ScraperBackgroundService : BackgroundService, IScraperBackgroundService
    {
        private readonly BlockingCollection<IScrapeJob> _queue = new BlockingCollection<IScrapeJob>();
        private readonly IServiceProvider _provider;
        private readonly ILogger<ScraperBackgroundService> _logger;

        public ScraperBackgroundService(IServiceProvider provider, ILogger<ScraperBackgroundService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var job = _queue.Take(stoppingToken);
                
                    using var scope = _provider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IScraperInternalService>();

                    try
                    {
                        switch (job)
                        {
                            case ScrapeAllJob all:
                                await service.ScrapeAsync(all.ScrapeId, stoppingToken);
                                break;
                        
                            case ScrapeForLocalMovieJob local:
                                await service.UpdateLocalMovieLibrariesAsync(stoppingToken);
                                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // TODO make configurable?
                                await service.ScrapeForLocalMovieAsync(local.MovieId, stoppingToken);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "background scrape job failed {type}", job?.GetType());   
                    }
                } 
            }, stoppingToken);
        }

        public void AddScrapeAllJob(int scrapeId) => _queue.Add(new ScrapeAllJob {ScrapeId = scrapeId});
        
        public void AddScrapeForLocalMovieJob(int movieId) => _queue.Add(new ScrapeForLocalMovieJob {MovieId = movieId});
        
        public override void Dispose()
        {
            base.Dispose();
            _queue.Dispose();
        }
        
        private interface IScrapeJob
        {
        }
    
        private class ScrapeAllJob : IScrapeJob
        {
            public int ScrapeId { get; set; }
        }

        private class ScrapeForLocalMovieJob : IScrapeJob
        {
            public int MovieId { get; set; }
        }
    }
}