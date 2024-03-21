using System.Reflection;
using MakeMovies.Api.Downloads;
using MakeMovies.Api.Movies;

namespace MakeMovies.Api.Tests;

public static class Fake
{
    public static string Id => Guid.NewGuid().ToString();

    public static Movie Movie => new(
        Id,
        "tt123456",
        "Some Movie",
        "some movie",
        "en",
        2024,
        6.6M,
        TimeSpan.FromMinutes(90),
        "Some movie description",
        new HashSet<string> { "action", "comedy" },
        Id,
        DateTime.UtcNow,
        [Torrent],
        InLibrary: false);

    public static Torrent Torrent => new(
        "abc123",
        "1080p",
        "bluray",
        6000,
        DateTime.UtcNow
    );

    public static Download Download => new(
        Id,
        Id,
        999,
        "Some Movie",
        DateTime.UtcNow,
        false
    );
    
    public static string Resource(string resourceName)
    {
        var fullyQualifiedName = $"{typeof(Fake).Namespace}.{resourceName}";
        using var stream = Assembly.GetAssembly(typeof(Fake))!
                               .GetManifestResourceStream(fullyQualifiedName)
                           ?? throw new Exception($"cannot find resource {fullyQualifiedName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}