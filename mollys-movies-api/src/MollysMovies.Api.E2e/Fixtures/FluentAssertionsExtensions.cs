using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Equivalency;
using FluentAssertions.Execution;
using FluentAssertions.Specialized;
using FluentValidation.Internal;
using MollysMovies.Client.Client;
using MollysMovies.Common.Mongo;
using MollysMovies.Common.Movies;
using WireMock.Server;
using ClientMovie = MollysMovies.Client.Model.Movie;

namespace MollysMovies.Api.E2e.Fixtures;

public static class FluentAssertionsExtensions
{
    public static WireMockAssertions Should(this WireMockServer instance) => new(instance);

    public static async Task ThrowApiExceptionAsync<TAssertions, TResult>(
        this AsyncFunctionAssertions<TResult, TAssertions> should,
        int expectedCode,
        params (string key, string message)[] expectedMessages)
        where TAssertions : AsyncFunctionAssertions<TResult, TAssertions> where TResult : Task
    {
        using var scope = new AssertionScope();
        var assertions = await should.ThrowAsync<ApiException>();
        assertions.And.ErrorCode.Should().Be(expectedCode);

        if (!expectedMessages.Any())
        {
            return;
        }

        var json = assertions.Which.ErrorContent.ToString()!;
        var content = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
        var expectedContent = expectedMessages
            .GroupBy(x => x.key)
            .ToDictionary(grp => grp.Key, grp => grp.Select(x => x.message).ToArray());
        content.Should().BeEquivalentTo(expectedContent, o => o.WithoutStrictOrdering());
    }

    public static EquivalencyAssertionOptions<TExpectation> ExcludingPropertiesOf<TExpectation, T>(
        this EquivalencyAssertionOptions<TExpectation> options,
        params Expression<Func<T, object?>>[] propertyExpressions)
    {
        var names = propertyExpressions.Select(expression =>
            expression.GetMember()?.Name ?? throw new Exception($"bad property expression {expression}"));
        return options.Excluding(m => m.DeclaringType == typeof(T) && names.Any(m.Path.EndsWith));
    }

    public static EquivalencyAssertionOptions<TExpectation> DatesToNearestSecond<TExpectation>(
        this EquivalencyAssertionOptions<TExpectation> options) =>
        options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
            .WhenTypeIs<DateTime>();

    public static EquivalencyAssertionOptions<Movie> ComparingToDto(this EquivalencyAssertionOptions<Movie> options) =>
        options.WithoutStrictOrdering()
            .DatesToNearestSecond()
            .Excluding(x => x.Meta)
            .Using(new MovieMetaEquivalencyStep());

    public static AndWhichConstraint<GenericCollectionAssertions<ClientMovie>, ClientMovie> ContainMovie(
        this GenericCollectionAssertions<ClientMovie> should, string title) =>
        should.ContainEquivalentOf(TestSeedData.Movie(title), o => o.ComparingToDto());

    public static AndConstraint<GenericCollectionAssertions<ClientMovie>> NotContainMovie(
        this GenericCollectionAssertions<ClientMovie> should, string title) =>
        should.NotContainEquivalentOf(TestSeedData.Movie(title), o => o.ComparingToDto());

    private class MovieMetaEquivalencyStep : IEquivalencyStep
    {
        public EquivalencyResult Handle(Comparands comparands, IEquivalencyValidationContext context, IEquivalencyValidator nestedValidator)
        {
            var subject = comparands.Subject as ClientMovie;
            var expected = comparands.Expectation as Movie;
            subject.Should().BeEquivalentTo(expected!.Meta!, o => o
                    .Excluding(x => x.Source)
                    .Excluding(x => x.RemoteImageUrl)
                    .Excluding(x => x.DateCreated)
                    .Excluding(x => x.DateScraped),
                context.Reason.FormattedMessage,
                context.Reason.Arguments);
            return EquivalencyResult.AssertionCompleted;
        }
    }
}