using System.Reflection;
using FluentAssertions.Execution;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MakeMovies.Api.Tests;

public static class WireMockExtensions
{
    public static IResponseBuilder WithJsonBodyFromResource(this IResponseBuilder builder, string resource) =>
        builder.WithHeader("Content-Type", "application/json")
            .WithBody(StringResource(resource));
    
    public static IResponseBuilder WithBinaryBodyFromResource(this IResponseBuilder builder, string resource, string contentType) =>
        builder.WithHeader("Content-Type", contentType)
            .WithBody(BinaryResource(resource));

    private static string StringResource(string resourceName)
    {
        using var stream = ResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    
    public static byte[] BinaryResource(string resourceName)
    {
        using var stream = ResourceStream(resourceName);
        var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static Stream ResourceStream(string resourceName) =>
        Assembly.GetAssembly(typeof(WireMockExtensions))!.GetManifestResourceStream(resourceName)
            ?? throw new Exception($"cannot find resource {resourceName}");
    
    
    public static WireMockAssertions Should(this IWireMockServer instance) => new(instance);
}

public class WireMockAssertions(IWireMockServer subject)
{
    public AndConstraint<WireMockAssertions> HaveCalledMapping(Guid mappingUuid, string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .Given(() => subject.LogEntries.Select(x => x.MappingGuid))
            .ForCondition(executedUuids => executedUuids.Contains(mappingUuid))
            .FailWith(
                "Expected {context:wiremockserver} to have received request for mapping {0}{reason}.",
                mappingUuid);

        return new AndConstraint<WireMockAssertions>(this);
    }
}