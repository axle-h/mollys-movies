using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.FakeData;
using RichardSzalay.MockHttp;

namespace MollysMovies.Test.Fixtures;

public sealed class ApiClientFixture<TSubject> where TSubject : class
{
    private ServiceProvider? _provider;

    public MockHttpMessageHandler MockHttp { get; } = new();

    public TSubject Subject =>
        _provider?.GetRequiredService<TSubject>() ?? throw new Exception("must call Configure()");

    public ApiClientFixture<TSubject> Configure(string baseAddress,
        Action<ServiceCollection>? configure = null,
        Func<MockHttpMessageHandler, HttpMessageHandler>? httpMessageHandlerFactory = null)
    {
        MockHttp.Clear();

        if (_provider is not null)
        {
            return this;
        }

        var services = new ServiceCollection();
        services.AddLogging()
            .AddOptions()
            .AddHttpClient<TSubject>(client => client.BaseAddress = new Uri(baseAddress))
            .ConfigurePrimaryHttpMessageHandler(() => httpMessageHandlerFactory?.Invoke(MockHttp) ?? MockHttp);
        configure?.Invoke(services);
        _provider = services.BuildServiceProvider();
        return this;
    }
}

public static class MockHttpMessageHandlerExtensions
{
    public static MockedRequest RespondWithJsonResource(this MockedRequest source, string resourceName) =>
        source.Respond(HttpStatusCode.OK, "application/json", Fake.Resource(resourceName));

    public static MockedRequest RespondWithXmlResource(this MockedRequest source, string resourceName) =>
        source.Respond(HttpStatusCode.OK, "text/xml", Fake.Resource(resourceName));
}