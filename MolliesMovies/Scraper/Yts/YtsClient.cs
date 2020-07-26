using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MolliesMovies.Common.ApiClient;
using MolliesMovies.Scraper.Yts.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Retry;

namespace MolliesMovies.Scraper.Yts
{
    public interface IYtsClient
    {
        Task<YtsListMoviesResponse> ListMoviesAsync(YtsListMoviesRequest request,
            CancellationToken cancellationToken = default);

        Task<YtsImage> GetImageAsync(string url, CancellationToken cancellationToken = default);
    }

    public class YtsClient : IYtsClient
    {
        private readonly JsonApiClient _ytsClient;
        private readonly HttpClient _client;
        private readonly AsyncRetryPolicy _clientPolicy;
        
        public YtsClient(HttpClient client, ILogger<YtsClient> logger)
        {
            _client = client;
            _ytsClient = new JsonApiClient(client, x =>
            {
                x.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };
                x.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            });
            _clientPolicy = Policy.Handle<ApiRequestException>()
                .WaitAndRetryAsync(
                    5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount) => logger.LogWarning(
                        exception,
                        "delaying for {delay}ms, then retry #{retry}.",
                        timeSpan.TotalMilliseconds, retryCount));
        }

        public async Task<YtsListMoviesResponse> ListMoviesAsync(YtsListMoviesRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = await _clientPolicy.ExecuteAsync(
                token => _ytsClient.GetAsync<YtsResponse<YtsListMoviesResponse>>("/api/v2/list_movies.json", request, token),
                cancellationToken
            );
            
            if (response.Status != "ok")
            {
                throw new Exception($"Failed to list YTS movies: {response.StatusMessage}");
            }

            return response.Data;
        }

        public async Task<YtsImage> GetImageAsync(string url, CancellationToken cancellationToken = default)
        {
            return await _clientPolicy.ExecuteAsync(async token =>
            {
                var response = await _client.GetAsync(url, token);
                response.EnsureSuccessStatusCode();

                return new YtsImage
                {
                    Content = await response.Content.ReadAsByteArrayAsync(),
                    ContentType = response.Content.Headers.ContentType.ToString(),
                };
            }, cancellationToken);
        }
    }
}