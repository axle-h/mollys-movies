using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.Api.Transmission;
using MollysMovies.Test.Fixtures;
using Xunit;

namespace MollysMovies.Test.Api.Transmission;

public class MagnetUriServiceTests : IClassFixture<AutoMockFixtureBuilder<MagnetUriService>>
{
    private readonly AutoMockFixture<MagnetUriService> _fixture;

    public MagnetUriServiceTests(AutoMockFixtureBuilder<MagnetUriService> builder)
    {
        _fixture = builder
            .Services(s => s.Configure<TransmissionOptions>(o =>
            {
                o.Trackers = new List<string> {"udp://tracker1:1337/announce", "udp://tracker2:1337/announce"};
            }))
            .Build();
    }

    [Fact]
    public void Builds_magnet_uri()
    {
        var observed = _fixture.Subject.BuildMagnetUri("Interstellar (2014)", "abc123");
        observed.Should()
            .Be(
                "magnet:?xt=urn:btih:abc123&dn=Interstellar%20%282014%29&tr=udp://tracker1:1337/announce&tr=udp://tracker2:1337/announce");
    }
}