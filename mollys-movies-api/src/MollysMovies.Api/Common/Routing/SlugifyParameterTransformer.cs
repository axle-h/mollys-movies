using Humanizer;
using Microsoft.AspNetCore.Routing;

namespace MollysMovies.Api.Common.Routing;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value) => value?.ToString()?.Kebaberize();
}