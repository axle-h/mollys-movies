using System.Net;
using System.Text;

namespace MollysMovies.FakeData;

public static class TestEnvironment
{
    public static string MongoUrl(string testId) => $"mongodb://{GetConfiguredHost("mongo")}:27017/{testId}";

    public static string RabbitMqUrl(string testId) => $"rabbitmq://user:password@{GetConfiguredHost("rabbitmq")}:5672/{testId}";
    
    public static async Task EnsureRabbitMqVirtualHostAsync(string testId)
    {
        using var handler = new HttpClientHandler { Credentials = new NetworkCredential { UserName = "user", Password = "password" } };
        using var client = new HttpClient(handler);
        var content = new StringContent("", Encoding.UTF8, "application/json");
        var result = await client.PutAsync($"http://{GetConfiguredHost("rabbitmq")}:15672/api/vhosts/{testId}", content);
        result.EnsureSuccessStatusCode();
    }
    
    private static string GetConfiguredHost(string name) =>
        Environment.GetEnvironmentVariable($"{name.ToUpper()}_HOST") ?? "localhost";
}