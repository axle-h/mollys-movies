using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace MakeMovies.Api.Downloads.TransmissionRpc;

public interface ITransmissionRpcClient
{
    Task<List<TorrentInfo>> GetAllTorrentsAsync(CancellationToken cancellationToken = default);

    Task<TorrentInfo?> GetTorrentByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<NewTorrentInfo> AddTorrentAsync(string uri, CancellationToken cancellationToken = default);

    Task RemoveTorrentAsync(int id, CancellationToken cancellationToken = default);
}

public class TransmissionRpcClient : ITransmissionRpcClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public TransmissionRpcClient(HttpClient client, IOptions<DownloadOptions> options)
    {
        _client = client;
        _client.BaseAddress = options.Value.Transmission.Url ?? throw new Exception("transmission url is required");
    }

    public static readonly ImmutableArray<string> AllFields = typeof(TorrentInfo)
        .GetProperties()
        .Select(x => char.ToLower(x.Name[0]) + x.Name[1..])
        .OrderBy(x => x)
        .ToImmutableArray();

    public async Task<List<TorrentInfo>> GetAllTorrentsAsync(CancellationToken cancellationToken = default)
    {
        var request = new TorrentGetRequest(null, AllFields);
        var response = await Rpc<TorrentGetRequest, TorrentGetResponse>("torrent-get", request, cancellationToken);
        return response.Torrents;
    }

    public async Task<TorrentInfo?> GetTorrentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new TorrentGetRequest(new[] {id}, AllFields);
        var response = await Rpc<TorrentGetRequest, TorrentGetResponse>("torrent-get", request, cancellationToken);
        return response.Torrents.FirstOrDefault();
    }

    public async Task<NewTorrentInfo> AddTorrentAsync(string uri, CancellationToken cancellationToken = default)
    {
        var request = new TorrentAddRequest(uri);
        var response = await Rpc<TorrentAddRequest, TorrentAddResponse>("torrent-add", request, cancellationToken);
        return response.TorrentAdded ?? response.TorrentDuplicate ?? throw new Exception($"failed to add torrent with uri {uri}");
    }

    public async Task RemoveTorrentAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = new TorrentRemoveRequest(new List<int> {id});
        await Rpc("torrent-remove", request, cancellationToken);
    }

    private async Task Rpc<TRequest>(string method, TRequest body, CancellationToken cancellationToken)
        where TRequest : class
    {
        await Rpc<TRequest, object>(method, body, cancellationToken);
    }

    private async Task<TResponse> Rpc<TRequest, TResponse>(string method, TRequest body, CancellationToken cancellationToken)
        where TRequest : class where TResponse : class
    {
        var wrapper = new TransmissionRequestWrapper<TRequest>(method, body);
        var request = new HttpRequestMessage(HttpMethod.Post, "rpc")
        {
            Content = JsonContent.Create(wrapper, new MediaTypeHeaderValue("application/json-rpc"), _jsonOptions)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<TransmissionResponseWrapper<TResponse>>(_jsonOptions, cancellationToken);
        if (content is null || content.Result != "success")
        {
            throw new Exception(
                $"transmission rpc {JsonSerializer.Serialize(wrapper, _jsonOptions)} failed with result {JsonSerializer.Serialize(content, _jsonOptions)}");
        }

        return content.Arguments;
    }

    private record TransmissionRequestWrapper<TArguments>(string Method, TArguments Arguments) where TArguments : class;

    private record TransmissionResponseWrapper<TArguments>(string Result, TArguments Arguments) where TArguments : class;

    private record TorrentGetRequest(IReadOnlyCollection<int>? Ids, IReadOnlyCollection<string> Fields);

    private record TorrentRemoveRequest(IReadOnlyCollection<int> Ids, [property: JsonPropertyName("delete-local-data")] bool DeleteData = false);

    private record TorrentGetResponse(List<TorrentInfo> Torrents);

    private record TorrentAddRequest(string Filename);

    private record TorrentAddResponse(
        [property: JsonPropertyName("torrent-added")] NewTorrentInfo? TorrentAdded,
        [property: JsonPropertyName("torrent-duplicate")] NewTorrentInfo? TorrentDuplicate);

    public class TransmissionRpcHandler : DelegatingHandler
    {
        private const string TransmissionSessionIdHeader = "X-Transmission-Session-Id";
        private static string? _currentSessionId;

        public TransmissionRpcHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_currentSessionId is not null)
            {
                request.Headers.Add(TransmissionSessionIdHeader, _currentSessionId);
            }

            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Conflict)
            {
                return response;
            }

            // session id is unset or stale
            _currentSessionId = response.Headers.GetValues(TransmissionSessionIdHeader).FirstOrDefault();
            request.Headers.Add(TransmissionSessionIdHeader, _currentSessionId);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}