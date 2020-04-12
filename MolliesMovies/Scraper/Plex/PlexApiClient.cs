using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MolliesMovies.Scraper.Plex.Models;
using Xml.Schema.Linq.Extensions;
using SectionsMediaContainer = Plex.Schema.Sections.MediaContainer;
using VideosMediaContainer = Plex.Schema.Videos.MediaContainer;

namespace MolliesMovies.Scraper.Plex
{
    public interface IPlexApiClient
    {
        Task<ICollection<PlexLibrary>> GetMovieLibrariesAsync(CancellationToken cancellationToken = default);

        Task<ICollection<PlexMovie>> GetMoviesAsync(string libraryKey, CancellationToken cancellationToken = default);

        Task UpdateLibraryAsync(string libraryKey, CancellationToken cancellationToken = default);

        Task<PlexImage> GetThumbAsync(string thumbPath, CancellationToken cancellationToken = default);
    }

    public class PlexApiClient : IPlexApiClient
    {
        private readonly HttpClient _client;
        private readonly IOptions<ScraperOptions> _options;

        public PlexApiClient(HttpClient client, IOptions<ScraperOptions> options)
        {
            _client = client;
            _options = options;
        }

        public async Task<ICollection<PlexLibrary>> GetMovieLibrariesAsync(CancellationToken cancellationToken = default)
        {
            var response = await _client.GetAsync(GetUrl("/library/sections"), cancellationToken);
            response.EnsureSuccessStatusCode();
            
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var container = SectionsMediaContainer.Load(reader);
            return container.Directory
                .Where(x => x.type == "movie" && !string.IsNullOrEmpty(x.key))
                .Select(x => new PlexLibrary { Key = x.key, Type = x.type})
                .ToList();
        }

        public async Task<ICollection<PlexMovie>> GetMoviesAsync(string libraryKey, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetAsync(GetUrl($"/library/sections/{libraryKey}/all"), cancellationToken);
            response.EnsureSuccessStatusCode();
            
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var container = VideosMediaContainer.Load(reader);
            return container.Video
                .Select(x => new PlexMovie
                {
                    Title = x.title,
                    Year = int.Parse(x.year),
                    ImdbCode = !Uri.TryCreate(x.guid, UriKind.Absolute, out var uri) || uri.Scheme != "com.plexapp.agents.imdb" ? null : uri.Host,
                    DateCreated = DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.addedAt)).UtcDateTime,
                    ThumbPath = x.thumb
                })
                .Where(x => !string.IsNullOrEmpty(x.ImdbCode))
                .ToList();
        }

        public async Task UpdateLibraryAsync(string libraryKey, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetAsync(GetUrl($"/library/sections/{libraryKey}/refresh"), cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task<PlexImage> GetThumbAsync(string thumbPath, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetAsync(GetUrl(thumbPath), cancellationToken);
            response.EnsureSuccessStatusCode();
            return new PlexImage
            {
                Content = await response.Content.ReadAsByteArrayAsync(),
                ContentType = response.Content.Headers.ContentType.ToString(),
            };
        }

        private string GetUrl(string path, IDictionary<string, object> query = null)
        {
            if (query is null)
            {
                query = new Dictionary<string, object>();
            }
            query.Add("X-Plex-Token", _options.Value.Plex.Token);

            var queryStrings = query
                .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value.ToString())}");

            return $"{path}?{string.Join('&', queryStrings)}";
        }
    }
}