using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MollysMovies.Callback;
using MollysMovies.ScraperClient;
using MollysMovies.Test.Fixtures;
using Moq;
using Xunit;

namespace MollysMovies.Test.Callback;

public class CallbackServiceTests : IClassFixture<AutoMockFixtureBuilder<CallbackService>>
{
    private readonly AutoMockFixture<CallbackService> _fixture;

    public CallbackServiceTests(AutoMockFixtureBuilder<CallbackService> builder)
    {
        _fixture = builder
            .InjectMock<IHostLifetime>()
            .InjectMock<IScraperClient>()
            .Services(s => s.Configure<TransmissionCallbackOptions>(o => o.TorrentId = "100"))
            .Build();
    }

    [Fact]
    public async Task Completes_callback()
    {
        _fixture.Mock<IScraperClient>(mock =>
            {
                mock.Setup(x => x.NotifyDownloadCompleteAsync("100", It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            })
            .Mock<IHostLifetime>(mock =>
            {
                mock.Setup(x => x.StopAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            });

        // the mocks complete synchronously so no need to mess around with async
        await _fixture.Subject.StartAsync(CancellationToken.None);

        _fixture.VerifyAll();
    }
    
    [Fact]
    public async Task Fails_to_complete_callback_still_stops_application()
    {
        _fixture.Mock<IScraperClient>(mock =>
            {
                mock.Setup(x => x.NotifyDownloadCompleteAsync("100", It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("notify failed"));
            })
            .Mock<IHostLifetime>(mock =>
            {
                mock.Setup(x => x.StopAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            });

        // the mocks complete synchronously so no need to mess around with async
        await _fixture.Subject.StartAsync(CancellationToken.None);

        _fixture.VerifyAll();
    }
}