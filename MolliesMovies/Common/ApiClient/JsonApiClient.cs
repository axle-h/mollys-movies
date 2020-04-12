using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MolliesMovies.Common.ApiClient
{
    public class JsonApiClient
    {
        private readonly HttpClient _client;
        private readonly JsonSerializer _serializer;

        public JsonApiClient(HttpClient client, Action<JsonSerializer> jsonConfigurator = null)
        {
            _client = client;
            _serializer = new JsonSerializer();
            jsonConfigurator?.Invoke(_serializer);
        }

        public async Task<TResponse> GetAsync<TResponse>(string url, object query = null, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response;
            try
            {
                response = await _client.GetAsync(url + GetQueryString(query), cancellationToken);
            }
            catch (Exception e)
            {
                throw new ApiRequestException(null, e);
            }

            return await DeserializeAsync<TResponse>(response);
        }

        private async Task<TResponse> DeserializeAsync<TResponse>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await ApiRequestException.FromResponseAsync(response);
            }

            try
            {
                using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                using var jsonReader = new JsonTextReader(reader);
                return _serializer.Deserialize<TResponse>(jsonReader);
            }
            catch (Exception e)
            {
                throw await ApiRequestException.FromResponseAsync(response, e);
            }
        }

        private string GetQueryString(object query)
        {
            if (query is null)
            {
                return string.Empty;
            }

            switch (_serializer.ContractResolver.ResolveContract(query.GetType()))
            {
                case JsonObjectContract joc:
                    var properties = joc.Properties
                        .Select(x => (name: x.PropertyName, value: x.ValueProvider.GetValue(query)))
                        .Where(x => !(x.value is null))
                        .Select(x => $"{x.name}={Uri.EscapeDataString(x.value.ToString())}")
                        .ToList();
                    if (!properties.Any())
                    {
                        return string.Empty;
                    }

                    return '?' + string.Join('&', properties);

                default:
                    throw new ArgumentException($"invalid object type {query.GetType()}");
            }
        }
    }
}