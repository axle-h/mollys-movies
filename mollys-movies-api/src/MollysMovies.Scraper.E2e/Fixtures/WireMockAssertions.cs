using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using WireMock.Server;

namespace MollysMovies.Scraper.E2e.Fixtures;

public class WireMockAssertions
{
    private readonly WireMockServer _subject;

    public WireMockAssertions(WireMockServer subject)
    {
        _subject = subject;
    }

    public AndConstraint<WireMockAssertions> HaveCalledMapping(Guid mappingUuid, string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .Given(() => _subject.LogEntries.Select(x => x.MappingGuid))
            .ForCondition(executedUuids => executedUuids.Contains(mappingUuid))
            .FailWith(
                "Expected {context:wiremockserver} to have received request for mapping {0}{reason}.",
                mappingUuid);

        return new AndConstraint<WireMockAssertions>(this);
    }
}