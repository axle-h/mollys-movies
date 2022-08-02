using System;
using System.IO;
using System.Linq;
using MollysMovies.FakeData;
using MollysMovies.Scraper.Plex.Models;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xml.Schema.Linq;
using VideosMediaContainer = Plex.Schema.Videos.MediaContainer;
using VideosMediaContainerType = Plex.Schema.Videos.MediaContainerType;
using VideosVideoType = Plex.Schema.Videos.VideoType;
using VideosTypeManager = Plex.Schema.Videos.LinqToXsdTypeManager;
using MetadataMediaContainer = Plex.Schema.Metadata.MediaContainer;
using MetadataMediaContainerType = Plex.Schema.Metadata.MediaContainerType;
using MetadataTypeManager = Plex.Schema.Metadata.LinqToXsdTypeManager;

namespace MollysMovies.Scraper.E2e;

public static class WireMockExtensions
{
    private const string PlexToken = "some-plex-token";
    private const string MovieLibraryKey = "5";
    private const string OtherLibraryKey = "4";

    public static WireMockServer GivenTransmissionRpc(this WireMockServer wireMock,
        string method,
        object arguments,
        object response,
        Guid? uuid = null)
    {
        wireMock.Given(
                Request.Create()
                    .WithPath("/transmission/rpc")
                    .WithHeader("Content-Type", "application/json-rpc")
                    .UsingPost()
                    .WithBody(new JsonPartialMatcher(new {method, arguments}))
            )
            .WithGuid(uuid ?? Guid.NewGuid())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new {result = "success", arguments = response})
            );

        return wireMock;
    }
    
    public static WireMockServer GivenPlexLibraries(this WireMockServer wireMock)
    {
        wireMock.Given(
                Request.Create()
                    .WithPath("/plex/library/sections")
                    .WithPlexToken()
                    .UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "text/xml")
                    .WithBody(Fake.Resource("Plex.sections.xml"))
            );
        return wireMock;
    }

    public static WireMockServer GivenPlexMovieLibrary(this WireMockServer wireMock, params PlexMovieMetadata[] movies)
    {
        var content = LoadAndMutateXml<VideosMediaContainer, VideosMediaContainerType>("Plex.movies.xml",
            VideosTypeManager.Instance,
            container =>
            {
                var template = container.Video.First();
                container.Video.Clear();
                container.size = movies.Length.ToString();

                foreach (var movie in movies)
                {
                    var video = (VideosVideoType) template.Clone();
                    video.ratingKey = movie.RatingKey;
                    video.addedAt = movie.DateCreated.ToPlexTime();
                    container.Video.Add(video);
                }
            });

        wireMock.Given(
                Request.Create()
                    .WithPath($"/plex/library/sections/{MovieLibraryKey}/all")
                    .WithPlexToken()
                    .UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "text/xml")
                    .WithBody(content)
            );

        var emptyLibrary = LoadAndMutateXml<VideosMediaContainer, VideosMediaContainerType>("Plex.movies.xml",
            VideosTypeManager.Instance, container => container.Video.Clear());
        wireMock.Given(
                Request.Create()
                    .WithPath($"/plex/library/sections/{OtherLibraryKey}/all")
                    .WithPlexToken()
                    .UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "text/xml")
                    .WithBody(emptyLibrary)
            );

        return wireMock;
    }

    public static WireMockServer GivenPlexMovieMetadata(this WireMockServer wireMock, string ratingKey,
        DateTime dateCreated, string imdbCode = "tt0144084", string title = "American Psycho", int year = 2000)
    {
        var plexTime = dateCreated.ToPlexTime();
        var content = LoadAndMutateXml<MetadataMediaContainer, MetadataMediaContainerType>("Plex.metadata.xml",
            MetadataTypeManager.Instance,
            container =>
            {
                var video = container.Video;
                var imdbGuid = video.Guid.First(x => x.id.StartsWith("imdb://"));
                imdbGuid.id = $"imdb://{imdbCode}";
                video.title = title;
                video.year = year.ToString();
                video.addedAt = plexTime;
                video.thumb = $"/library/metadata/{ratingKey}/thumb/{plexTime}";
            });

        wireMock.Given(
                Request.Create()
                    .WithPath($"/plex/library/metadata/{ratingKey}")
                    .WithPlexToken()
                    .UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "text/xml")
                    .WithBody(content)
            );

        return wireMock;
    }

    public static WireMockServer GivenPlexUpdateMovieLibrary(this WireMockServer wireMock)
    {
        wireMock.Given(
                Request.Create()
                    .WithPath($"/plex/library/sections/{MovieLibraryKey}/refresh",
                        $"/plex/library/sections/{OtherLibraryKey}/refresh")
                    .WithPlexToken()
                    .UsingGet()
            )
            .RespondWith(Response.Create().WithStatusCode(200));

        return wireMock;
    }

    public static WireMockServer GivenYtsListMovies(this WireMockServer wireMock)
    {
        wireMock.Given(
                Request.Create()
                    .WithPath("/yts/api/v2/list_movies.json")
                    .WithParam("page", "1")
                    .WithParam("limit", "50")
                    .WithParam("order_by", "desc")
                    .WithParam("sort_by", "date_added")
                    .UsingGet()
            )
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "text/xml")
                .WithBody(Fake.Resource("Yts.list_movies.json")));

        wireMock.Given(
                Request.Create()
                    .WithPath("/yts/api/v2/list_movies.json")
                    .WithParam("page", "2")
                    .WithParam("limit", "50")
                    .WithParam("order_by", "desc")
                    .WithParam("sort_by", "date_added")
                    .UsingGet()
            )
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "text/xml")
                .WithBody(Fake.Resource("Yts.list_movies_empty.json")));

        return wireMock;
    }

    public static WireMockServer GivenYtsImages(this WireMockServer wireMock)
    {
        foreach (var name in new[] {"christmas_with_the_chosen_the_messengers_2021", "battle_of_dunkirk_from_disaster_to_triumph_2018"})
        {
            wireMock.Given(
                    Request.Create()
                        .WithPath($"/yts/assets/images/movies/{name}/large-cover.jpg")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "image/jpeg")
                    .WithBody(name));
        }
        return wireMock;
    }

    private static string ToPlexTime(this DateTime date) =>
        new DateTimeOffset(date, TimeSpan.Zero).ToUnixTimeSeconds().ToString();

    private static IRequestBuilder WithPlexToken(this IRequestBuilder builder) =>
        builder.WithParam("X-Plex-Token", PlexToken);

    private static string LoadAndMutateXml<TContainer, TContainerType>(string resourceName,
        ILinqToXsdTypeManager typeManager,
        Action<TContainer> mutateAction)
        where TContainer : XTypedElement
        where TContainerType : XTypedElement
    {
        var xml = Fake.Resource(resourceName);
        var container = XTypedServices.Parse<TContainer, TContainerType>(xml, typeManager);
        mutateAction(container);
        using var writer = new StringWriter();
        XTypedServices.Save(writer, container.Untyped);
        return writer.ToString();
    }
}