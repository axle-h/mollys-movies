using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MollysMovies.Test.Fixtures;

public sealed class AutoMockFixture<TSubject> where TSubject : class
{
    private readonly ReadOnlyDictionary<Type, Mock> _mocks;
    private readonly IReadOnlyCollection<Action<AutoMockFixture<TSubject>>> _resetActions;

    public AutoMockFixture(
        IReadOnlyCollection<Action<AutoMockFixture<TSubject>>> resetActions,
        ReadOnlyDictionary<Type, Mock> mocks,
        ServiceProvider provider)
    {
        _resetActions = resetActions;
        _mocks = mocks;
        Provider = provider;
    }

    public ServiceProvider Provider { get; }

    public TSubject Subject => Provider.GetRequiredService<TSubject>();

    public Mock<TMock> GetMock<TMock>() where TMock : class =>
        _mocks.TryGetValue(typeof(TMock), out var mock)
            ? mock.As<TMock>()
            : throw new Exception($"no mock configured for {typeof(TMock)}");

    public AutoMockFixture<TSubject> Mock<TMock>(Action<Mock<TMock>> configure)
        where TMock : class
    {
        if (!_mocks.TryGetValue(typeof(TMock), out var mock))
        {
            throw new Exception($"no mock configured for {typeof(TMock)}");
        }

        configure(mock.As<TMock>());
        return this;
    }

    public AutoMockFixture<TSubject> Reset()
    {
        foreach (var mock in _mocks.Values)
        {
            mock.Reset();
        }

        foreach (var action in _resetActions)
        {
            action(this);
        }

        return this;
    }

    public AutoMockFixture<TSubject> VerifyAll()
    {
        foreach (var mock in _mocks.Values)
        {
            mock.VerifyAll();
        }

        return this;
    }
}

public sealed class AutoMockFixtureBuilder<TSubject> where TSubject : class
{
    private readonly Lazy<AutoMockFixture<TSubject>> _fixture;
    private readonly Dictionary<Type, Mock> _mocks = new();
    private readonly List<Action<AutoMockFixture<TSubject>>> _resetActions = new();

    private readonly IServiceCollection _services = new ServiceCollection()
        .AddSingleton<TSubject>()
        .AddOptions()
        .AddLogging();

    public AutoMockFixtureBuilder()
    {
        _fixture = new Lazy<AutoMockFixture<TSubject>>(() => new AutoMockFixture<TSubject>(
            _resetActions.AsReadOnly(),
            new ReadOnlyDictionary<Type, Mock>(_mocks),
            _services.BuildServiceProvider()
        ));
    }

    private bool IsBuilt => _fixture.IsValueCreated;

    public AutoMockFixtureBuilder<TSubject> Services(Action<IServiceCollection> configureServices)
    {
        if (IsBuilt)
        {
            return this;
        }

        configureServices(_services);
        return this;
    }

    public AutoMockFixtureBuilder<TSubject> InjectMock<TAs, TMock>()
        where TAs : class
        where TMock : class, TAs
    {
        if (IsBuilt || _mocks.TryGetValue(typeof(TMock), out var mock))
        {
            return this;
        }

        mock = new Mock<TMock>(MockBehavior.Strict);
        _mocks.Add(typeof(TMock), mock);
        _services.AddSingleton(mock.As<TAs>().Object);
        return this;
    }

    public AutoMockFixtureBuilder<TSubject> InjectMock<TMock>()
        where TMock : class =>
        InjectMock<TMock, TMock>();

    public AutoMockFixtureBuilder<TSubject> GlobalMock<TMock>(Action<Mock<TMock>> configure)
        where TMock : class =>
        InjectMock<TMock>().WhenReset(f => f.Mock(configure));

    public AutoMockFixtureBuilder<TSubject> WhenReset(Action<AutoMockFixture<TSubject>> resetAction)
    {
        if (IsBuilt)
        {
            return this;
        }

        _resetActions.Add(resetAction);
        return this;
    }

    public AutoMockFixture<TSubject> Build() => _fixture.Value.Reset();
}