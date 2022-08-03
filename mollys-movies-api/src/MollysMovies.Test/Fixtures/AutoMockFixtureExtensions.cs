using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.Common;
using MollysMovies.FakeData;

namespace MollysMovies.Test.Fixtures;

public static class AutoMockFixtureExtensions
{
    public static AutoMockFixtureBuilder<TSubject> MockSystemClock<TSubject>(
        this AutoMockFixtureBuilder<TSubject> builder,
        DateTime? utcNow = null)
        where TSubject : class =>
        builder.GlobalMock<ISystemClock>(mock => mock.Setup(x => x.UtcNow).Returns(utcNow ?? Fake.UtcNow));

    public static AutoMockFixture<TSubject> MockSystemClock<TSubject>(this AutoMockFixture<TSubject> fixture,
        DateTime? utcNow = null)
        where TSubject : class =>
        fixture.Mock<ISystemClock>(mock => mock.Setup(x => x.UtcNow).Returns(utcNow ?? Fake.UtcNow));

    public static AutoMockFixtureBuilder<TSubject> InjectFileSystem<TSubject>(
        this AutoMockFixtureBuilder<TSubject> builder, Action<MockFileSystem>? configureFileSystem = null)
        where TSubject : class =>
        builder.Services(services => services.AddSingleton<IFileSystem>(new MockFileSystem()))
            .WhenReset(p =>
            {
                var fileSystem = p.FileSystem();
                fileSystem.AllNodes.ToList().ForEach(fileSystem.RemoveFile);
                configureFileSystem?.Invoke(fileSystem);
            });

    public static MockFileSystem FileSystem<TSubject>(this AutoMockFixture<TSubject> fixture)
        where TSubject : class =>
        (MockFileSystem) fixture.Provider.GetRequiredService<IFileSystem>();
}