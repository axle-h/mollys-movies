using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace MakeMovies.Api.Library.Jellyfin;

public class JellyfinClient : ILibrarySource
{
    private readonly HttpClient _client;
    
    public JellyfinClient(HttpClient client, IOptions<LibraryOptions> options)
    {
        var jellyfinOptions = options.Value.Jellyfin;
        var apiKey = jellyfinOptions?.ApiKey ?? throw new Exception("Jellyfin ApiKey is required");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Mediabrowser", $"Token=\"{apiKey}\"");
        client.BaseAddress = jellyfinOptions.Url ?? throw new Exception("Jellyfin Url is required");
        _client = client;
    }
    
    public async Task<ICollection<LibraryMovie>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var adminUser = await GetAdminUserAsync(cancellationToken);
        if (adminUser is null)
        {
            return [];
        }
        
        var response = await _client.GetAsync($"Users/{adminUser.Id}/Items?hasImdbId=true&fields=ProviderIds&includeItemTypes=Movie&recursive=true", cancellationToken);
        response.EnsureSuccessStatusCode();

        var movies = await response.Content.ReadFromJsonAsync<GetItemsResponse>(cancellationToken);
        return movies?.Items.Select(ToLibraryMovie).ToList() ?? [];
    }

    public async Task UpdateLibraryAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsync("Library/Refresh", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<User?> GetAdminUserAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync("Users", cancellationToken);
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<User>>(cancellationToken);
        return users is null || users.Count == 0 ? null
            : users.Find(u => u.Policy.IsAdministrator) ?? users.First();
    }
    
    private static LibraryMovie ToLibraryMovie(Item item) => new(item.ProviderIds.Imdb);

    private record User(string Id, UserPolicy Policy);

    private record UserPolicy(bool IsAdministrator);

    private record GetItemsResponse(List<Item> Items);
    
    private record Item(ProviderIds ProviderIds);

    private record ProviderIds(string Imdb);

}