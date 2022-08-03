using Bogus;
using MongoDB.Bson;

namespace MollysMovies.FakeData;

public static class FakerExtensions
{
    public static Faker<T> With<T>(this Faker<T> faker, Action<Faker, T> action) where T : class =>
        faker.Clone().FinishWith(action);

    public static Faker<T> With<T>(this Faker<T> faker, Action<T> action) where T : class =>
        faker.With((_, t) => action(t));

    public static int Id(this Faker f) => f.Random.Number(1, int.MaxValue);

    public static string MongoId(this Faker _) => ObjectId.GenerateNewId().ToString();

    public static string SourceName(this Faker f) => f.PickRandom("Yts", "Plex", "PirateBay");

    public static string Genre(this Faker f) => f.Genres().First();

    public static ICollection<string> Genres(this Faker f) =>
        f.PickRandom(new[] {"Action", "Comedy", "Drama", "Fantasy", "Horror", "Romance", "Thriller"}, 2).ToList();

    public static string ImdbCode(this Faker f) => $"tt{f.Random.String2(7, "0123456789")}";

    public static string TorrentHash(this Faker f) => f.Random.AlphaNumeric(32).ToUpper();

    public static string YoutubeVideoId(this Faker f) => $"{f.Random.String2(6)}-${f.Random.AlphaNumeric(4)}";

    public static string LanguageCode(this Faker f) => f.PickRandom("de", "en", "es", "fr", "zh");

    public static string MpaaRating(this Faker f) => f.PickRandom("G", "PG", "PG-13", "R", "NC-17");

    public static string MovieQuality(this Faker f) => f.PickRandom("720p", "1080p", "2160p", "3D");

    public static string MovieType(this Faker f) => f.PickRandom("bluray", "web");

    public static decimal MovieRating(this Faker f) => Math.Round(f.Random.Decimal(0, 10), 1);

    public static string MovieName(this Faker f) => f.PickRandom(
        "Back to the Future",
        "Desperado",
        "Night at the Museum",
        "Robocop",
        "Ghostbusters",
        "Cool World",
        "Donnie Darko",
        "Double Indemnity",
        "The Spanish Prisoner",
        "The Smurfs",
        "Dead Alive",
        "Army of Darkness",
        "Peter Pan",
        "The Jungle Story",
        "Red Planet",
        "Deep Impact",
        "The Long Kiss Goodnight",
        "Juno",
        "(500) Days of Summer",
        "The Dark Knight",
        "Bringing Down the House",
        "Se7en",
        "Chocolat",
        "The American",
        "The American President",
        "Hudsucker Proxy",
        "Conan the Barbarian",
        "Shrek",
        "The Fox and the Hound",
        "Lock, Stock, and Two Barrels",
        "Date Night",
        "200 Cigarettes",
        "9 1/2 Weeks",
        "Iron Man 2",
        "Tombstone",
        "Young Guns",
        "Fight Club",
        "The Cell",
        "The Unborn",
        "Black Christmas",
        "The Change-Up",
        "The Last of the Mohicans",
        "Shutter Island",
        "Ronin",
        "Ocean’s 11",
        "Philadelphia",
        "Chariots of Fire",
        "M*A*S*H",
        "Walking and Talking",
        "Walking Tall",
        "The 40 Year Old Virgin",
        "Superman III",
        "The Hour",
        "The Slums of Beverly Hills",
        "Secretary",
        "Secretariat",
        "Pretty Woman",
        "Sleepless in Seattle",
        "The Iron Mask",
        "Smoke",
        "Schindler’s List",
        "The Beverly Hillbillies",
        "The Ugly Truth",
        "Bounty Hunter",
        "Say Anything",
        "8 Seconds",
        "Metropolis",
        "Indiana Jones and the Temple of Doom",
        "Kramer vs. Kramer",
        "The Manchurian Candidate",
        "Raging Bull",
        "Heat",
        "About Schmidt",
        "Re-Animator",
        "Evolution",
        "Gone in 60 Seconds",
        "Wanted",
        "The Man with One Red Shoe",
        "The Jerk",
        "Whip It",
        "Spanking the Monkey",
        "Steel Magnolias",
        "Horton Hears a Who",
        "Honey",
        "Brazil",
        "Gorillas in the Mist",
        "Before Sunset",
        "After Dark",
        "From Dusk til Dawn",
        "Cloudy with a Chance of Meatballs",
        "Harvey",
        "Mr. Smith Goes to Washington",
        "L.A. Confidential",
        "Little Miss Sunshine",
        "The Future",
        "Howard the Duck",
        "Howard’s End",
        "The Innkeeper",
        "Revolutionary Road",
        "Interstellar"
    );

    public static int MovieYear(this Faker f) => f.Random.Number(1980, 2021);

    public static string MovieNameWithYear(this Faker f) => $"{f.MovieName()} ({f.MovieYear()})";

    public static string MagnetUri(this Faker f) =>
        $"magnet:?xt=urn:btih:{f.TorrentHash()}&dn={Uri.EscapeDataString(f.MovieNameWithYear())}&tr={f.Internet.UrlWithPath()}&tr={f.Internet.UrlWithPath()}";

    public static long SizeBytes(this Faker f) => f.Random.Long(104900000, 1074000000);
}