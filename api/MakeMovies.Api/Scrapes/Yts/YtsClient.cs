using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace MakeMovies.Api.Scrapes.Yts;

public interface IYtsClient
{
    Task<YtsListMoviesResponse> ListMoviesAsync(int page, int limit, CancellationToken cancellationToken = default);
}

public class YtsClient : IYtsClient
{
    private readonly HttpClient _client;
    private readonly AsyncRetryPolicy _clientPolicy;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public YtsClient(HttpClient client, IOptions<ScrapeOptions> options, ILogger<YtsClient> logger)
    {
        var ytsOptions = options.Value.Yts;
        _client = client;
        _clientPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(
                ytsOptions.MaxRetries,
                _ => ytsOptions.RetryDelay,
                (exception, timeSpan, retryCount) => logger.LogWarning(
                    exception,
                    "delaying for {delay}ms, then retry #{retry}.",
                    timeSpan.TotalMilliseconds, retryCount));
        _client.BaseAddress = ytsOptions.Url ?? throw new Exception("YTS url is required");
    }
    
    public async Task<YtsListMoviesResponse> ListMoviesAsync(int page, int limit, CancellationToken cancellationToken = default) =>
        await _clientPolicy.ExecuteAsync(async token => await ListMoviesRawAsync(page, limit, token), cancellationToken);
    
    private async Task<YtsListMoviesResponse> ListMoviesRawAsync(int page, int limit, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync(
            $"api/v2/list_movies.json?page={page}&limit={limit}&sort_by=date_added&order_by=asc",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var content =
            await response.Content
                .ReadFromJsonAsync<YtsResponseWrapper<YtsListMoviesResponse>>(JsonOptions, cancellationToken);
        return content?.Data ?? throw new Exception("failed to deserialize list movies response");
    }
    
    // ReSharper disable once ClassNeverInstantiated.Local
    private record YtsResponseWrapper<TData>(string Status, string StatusMessage, TData Data);
}