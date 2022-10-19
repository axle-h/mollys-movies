using System;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MollysMovies.Scraper.Yts.Models;
using Polly;
using Polly.Retry;

namespace MollysMovies.Scraper.Yts;

public interface IYtsClient
{
    Task HealthCheckAsync(CancellationToken cancellationToken);

    Task<YtsListMoviesResponse> ListMoviesAsync(YtsListMoviesRequest request,
        CancellationToken cancellationToken = default);

    Task<YtsImage> GetImageAsync(string url, CancellationToken cancellationToken = default);
}

/// <summary>
///     Client for the YTS API.
/// </summary>
public class YtsClient : IYtsClient
{
    private readonly HttpClient _client;
    private readonly AsyncRetryPolicy _clientPolicy;
    private readonly ScraperOptions _options;
    private readonly IFileSystem _fileSystem;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = {new DateTimeConverterUsingDateTimeParse()}
    };

    public YtsClient(HttpClient client, ILogger<YtsClient> logger, IOptions<ScraperOptions> options, IFileSystem fileSystem)
    {
        _options = options.Value;
        _client = client;
        _fileSystem = fileSystem;
        _clientPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(
                5,
                _ => _options.Yts?.RetryDelay ?? TimeSpan.Zero,
                (exception, timeSpan, retryCount) => logger.LogWarning(
                    exception,
                    "delaying for {delay}ms, then retry #{retry}.",
                    timeSpan.TotalMilliseconds, retryCount));
    }

    public async Task HealthCheckAsync(CancellationToken cancellationToken)
    {
        var response = await ListMoviesRawAsync(new YtsListMoviesRequest {Page = 1, Limit = 1}, cancellationToken);
        var success = response.Movies?.Any() ?? false;
        if (!success)
        {
            throw new Exception(
                $"yts list movies service responded with empty list of movies {JsonSerializer.Serialize(response)}");
        }
    }

    public async Task<YtsListMoviesResponse> ListMoviesAsync(YtsListMoviesRequest request,
        CancellationToken cancellationToken = default) =>
        await _clientPolicy.ExecuteAsync(async token => await ListMoviesRawAsync(request, token), cancellationToken);

    public async Task<YtsImage> GetImageAsync(string url, CancellationToken cancellationToken = default)
    {
        return await _clientPolicy.ExecuteAsync(async token =>
        {
            var response = await _client.GetAsync(GetUrl(url, new { }), token);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.ToString() ??
                              throw new Exception($"no content type on image '{url}'");
            return new YtsImage(content, contentType);
        }, cancellationToken);
    }

    private async Task<YtsListMoviesResponse> ListMoviesRawAsync(YtsListMoviesRequest request, CancellationToken cancellationToken)
    {
        var uri = GetUrl("api/v2/list_movies.json", request);
        var response = await _client.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (_options.Yts?.DumpJson == true)
        {
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(_options.DownloadsPath, $"yts_list_movies_{request.Page}.json"), json, cancellationToken);
        }

        var content =
            await response.Content
                .ReadFromJsonAsync<YtsResponseWrapper<YtsListMoviesResponse>>(_jsonOptions, cancellationToken);
        return content?.Data ?? throw new Exception("failed to deserialize list movies response");
    }

    private string GetUrl<TQuery>(string path, TQuery query) where TQuery : class
    {
        var relativePath = path.TrimStart('/'); // so base path is used
        var queryTokens = JsonSerializer.SerializeToNode(query, _jsonOptions)
            ?.AsObject()
            .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value?.ToString() ?? "")}")
            .ToList();
        return queryTokens is null || !queryTokens.Any()
            ? relativePath
            : $"{relativePath}?{string.Join("&", queryTokens)}";
    }

    private class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.Underscore();
    }

    private class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            DateTime.Parse(reader.GetString() ?? "");

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}