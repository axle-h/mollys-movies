using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentValidation.Internal;
using WireMock.Server;

namespace MollysMovies.Scraper.E2e.Fixtures;

public static class FluentAssertionsExtensions
{
    public static WireMockAssertions Should(this WireMockServer instance) => new(instance);
    
    public static EquivalencyAssertionOptions<TExpectation> DatesToNearestSecond<TExpectation>(
        this EquivalencyAssertionOptions<TExpectation> options) =>
        options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
            .WhenTypeIs<DateTime>();

    public static EquivalencyAssertionOptions<TExpectation> ExcludingPropertiesOf<TExpectation, T>(
        this EquivalencyAssertionOptions<TExpectation> options,
        params Expression<Func<T, object?>>[] propertyExpressions)
    {
        var names = propertyExpressions.Select(expression =>
            expression.GetMember()?.Name ?? throw new Exception($"bad property expression {expression}"));
        return options.Excluding(m => m.DeclaringType == typeof(T) && names.Any(m.Path.EndsWith));
    }
}