using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MollysMovies.FakeData;
using MollysMovies.ScraperClient;
using Xunit;

namespace MollysMovies.Callback.E2e;

public class CallbackTests
{
    [Fact]
    public async Task Successfully_notifies()
    {
        var rabbitMqUrl = TestEnvironment.RabbitMqUrl("callback");
        var builder = Program.GetHostBuilder(new[] {"--TorrentId", "some-torrent-id"})
            .ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:rabbitmq"] = rabbitMqUrl
            }));
        await TestEnvironment.EnsureRabbitMqVirtualHostAsync("callback");
        
        using var rabbitMq = await new RabbitMqTestHarness.Builder(rabbitMqUrl)
            .Consume<NotifyDownloadComplete>()
            .RunAsync();

        using var host = builder.Build();
        await host.StartAsync();

        var notifications = rabbitMq.Consumed<NotifyDownloadComplete>();
        notifications.Should().Be(new NotifyDownloadComplete("some-torrent-id"));
    }
}