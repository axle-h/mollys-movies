using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MollysMovies.Common.Movies;
using MollysMovies.Common.Scraper;

namespace MollysMovies.Common.Mongo;

public static class TestSeedData
{
    public static ICollection<Scrape> Scrapes => new List<Scrape>
    {
        // Success
        new()
        {
            Id = "507f1f77bcf86cd799439011",
            Success = true,
            StartDate = DateTime.Parse("2022-01-14T10:17:54.448070"),
            EndDate = DateTime.Parse("2022-01-14T10:23:31.130799"),
            MovieCount = 188,
            TorrentCount = 378,
            Sources = new List<ScrapeSource>
            {
                new()
                {
                    Type = ScraperType.Local,
                    StartDate = DateTime.Parse("2022-01-14T10:18:03.057996"),
                    EndDate = DateTime.Parse("2022-01-14T10:18:03.479710"),
                    Source = "plex",
                    Success = true
                },
                new()
                {
                    Type = ScraperType.Torrent,
                    StartDate = DateTime.Parse("2022-01-14T10:17:54.459980"),
                    EndDate = DateTime.Parse("2022-01-14T10:18:03.057994"),
                    Source = "yts",
                    Success = true,
                    MovieCount = 188,
                    TorrentCount = 378
                }
            }
        },
        // Failed
        new()
        {
            Id = "507f1f77bcf86cd799439012",
            Success = false,
            StartDate = DateTime.Parse("2021-12-25 21:29:44.786585"),
            EndDate = DateTime.Parse("2021-12-25 21:31:01.464273"),
            Sources = new List<ScrapeSource>
            {
                new()
                {
                    Type = ScraperType.Local,
                    StartDate = DateTime.Parse("2021-12-25 21:30:47.093379"),
                    EndDate = DateTime.Parse("2021-12-25 21:30:48.002272"),
                    Source = "plex",
                    Success = true
                },
                new()
                {
                    Type = ScraperType.Torrent,
                    StartDate = DateTime.Parse("2021-12-25 21:29:44.830298"),
                    EndDate = DateTime.Parse("2021-12-25 21:30:47.092975"),
                    Source = "yts",
                    Success = false,
                    Error = "API request failed"
                }
            }
        }
    };

    public static ICollection<Movie> Movies => ParseSeedData<Movie>("test-seed.json");

    public static Movie Movie(string title) =>
        Movies.FirstOrDefault(x => x.Meta!.Title == title) ??
        throw new Exception($"movie not found with title '{title}'");

    private static ICollection<T> ParseSeedData<T>(string resourceName)
    {
        using var stream = Assembly.GetExecutingAssembly()
                               .GetManifestResourceStream($"{typeof(TestSeedData).Namespace}.{resourceName}")
                           ?? throw new Exception($"cannot find resource {resourceName}");
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        var options = new JsonSerializerOptions {Converters = {new JsonStringEnumConverter(), new DateTimeConverterUsingDateTimeParse()}};
        return JsonSerializer.Deserialize<ICollection<T>>(content, options) ??
               throw new Exception($"failed to parse {resourceName}");
    }

    private class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = DateTime.Parse(reader.GetString()!);
            return value.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}