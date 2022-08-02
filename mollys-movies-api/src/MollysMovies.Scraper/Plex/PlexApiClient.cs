using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MollysMovies.Scraper.Plex.Models;
using Plex.Schema.Metadata;
using SectionsMediaContainer = Plex.Schema.Sections.MediaContainer;
using VideosMediaContainer = Plex.Schema.Videos.MediaContainer;

namespace MollysMovies.Scraper.Plex;

public interface IPlexApiClient
{
    Task<ICollection<PlexLibrary>> GetMovieLibrariesAsync(CancellationToken cancellationToken = default);

    Task<ICollection<PlexMovieMetadata>> GetAllMovieMetadataAsync(string libraryKey,
        CancellationToken cancellationToken = default);

    Task UpdateLibraryAsync(string libraryKey, CancellationToken cancellationToken = default);

    Task<PlexImage> GetThumbAsync(string thumbPath, CancellationToken cancellationToken = default);

    Task<PlexMovie?> GetMovieAsync(string ratingKey, CancellationToken cancellationToken = default);
}

public class PlexApiClient : IPlexApiClient
{
    private readonly HttpClient _client;
    private readonly string _token;

    public PlexApiClient(HttpClient client, IOptions<ScraperOptions> options)
    {
        _client = client;
        _token = options.Value.Plex?.Token ?? throw new ArgumentException("plex token is required");
    }

    public async Task<ICollection<PlexLibrary>> GetMovieLibrariesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(GetUrl("library/sections"), cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(content);
        var container = SectionsMediaContainer.Load(reader);
        return container.Directory
            .Where(x => !string.IsNullOrEmpty(x.key) && x.type == "movie")
            .Select(x => new PlexLibrary(x.key, x.type))
            .ToList();
    }

    public async Task<ICollection<PlexMovieMetadata>> GetAllMovieMetadataAsync(string libraryKey,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(GetUrl($"library/sections/{libraryKey}/all"), cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(content);
        var container = VideosMediaContainer.Load(reader);
        return container.Video
            .Select(x =>
                new PlexMovieMetadata(x.ratingKey,
                    DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.addedAt)).UtcDateTime)
            )
            .ToList();
    }

    public async Task UpdateLibraryAsync(string libraryKey, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(GetUrl($"library/sections/{libraryKey}/refresh"), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PlexMovie?> GetMovieAsync(string ratingKey, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(GetUrl($"library/metadata/{ratingKey}"), cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(content);
        var container = MediaContainer.Load(reader);
        var video = container.Video;

        var imdbCode = video.Guid
            .Select(x => Uri.TryCreate(x.id, UriKind.Absolute, out var uri) && uri.Scheme == "imdb" ? uri.Host : null)
            .FirstOrDefault();

        if (imdbCode is null)
        {
            // a movie must have an imdb code
            return null;
        }

        return new PlexMovie(imdbCode, video.title, int.Parse(video.year),
            DateTimeOffset.FromUnixTimeSeconds(long.Parse(video.addedAt)).UtcDateTime, video.thumb);
    }

    public async Task<PlexImage> GetThumbAsync(string thumbPath, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(GetUrl(thumbPath), cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString() ??
                          throw new Exception($"no content type on image '{thumbPath}'");
        return new PlexImage(content, contentType);
    }

    private string GetUrl(string path) => $"{path.TrimStart('/')}?X-Plex-Token={Uri.EscapeDataString(_token)}";
}