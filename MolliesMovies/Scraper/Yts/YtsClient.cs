using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MolliesMovies.Common.ApiClient;
using MolliesMovies.Scraper.Yts.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MolliesMovies.Scraper.Yts
{
    public interface IYtsClient
    {
        Task<YtsListMoviesResponse> ListMoviesAsync(YtsListMoviesRequest request,
            CancellationToken cancellationToken = default);
    }

    public class YtsClient : IYtsClient
    {
        private readonly JsonApiClient _client;
        private readonly ILogger<YtsClient> _logger;
        
        public YtsClient(HttpClient client, ILogger<YtsClient> logger)
        {
            _logger = logger;
            _client = new JsonApiClient(client, x =>
            {
                x.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };
                x.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            });
        }

        public async Task<YtsListMoviesResponse> ListMoviesAsync(YtsListMoviesRequest request,
            CancellationToken cancellationToken = default)
        {
            // TODO replace with polly
            Exception lastException = null;
            YtsResponse<YtsListMoviesResponse> response = null;
            for (var retry = 0; retry < 7; retry++)
            {
                if (retry > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry)), cancellationToken);
                }
                
                try
                {
                    response = await _client.GetAsync<YtsResponse<YtsListMoviesResponse>>("/api/v2/list_movies.json", request,
                        cancellationToken);
                    break;
                }
                catch (Exception e)
                {
                    lastException = e;
                    _logger.LogError(e, "[attempt: {attempt}] failed to scrape {page}", retry + 1, request.Page);
                }
            }

            if (response is null)
            {
                throw lastException ?? new Exception("failed to scrape yts");
            }
            
            if (response.Status != "ok")
            {
                throw new Exception($"Failed to list YTS movies: {response.StatusMessage}");
            }

            return response.Data;
        }
    }
}