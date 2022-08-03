using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MollysMovies.ScraperClient;

namespace MollysMovies.Callback;

public class CallbackService : BackgroundService
{
    private readonly IHostLifetime _lifetime;
    private readonly ILogger<CallbackService> _logger;
    private readonly IScraperClient _scraperClient;
    private readonly TransmissionCallbackOptions _options; 
    
    public CallbackService(IHostLifetime lifetime, ILogger<CallbackService> logger, IScraperClient scraperClient, IOptions<TransmissionCallbackOptions> options)
    {
        _lifetime = lifetime;
        _logger = logger;
        _scraperClient = scraperClient;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_options.TorrentId))
            {
                throw new Exception("A torrent id is required");
            }
        
            // TODO retry if fails
            await _scraperClient.NotifyDownloadCompleteAsync(_options.TorrentId, stoppingToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "callback failed");
        }
        finally
        {
            await _lifetime.StopAsync(CancellationToken.None);
        }
    }
}