using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MollysMovies.Scraper.E2e;

public class RabbitMqTestHarness : IDisposable
{
    private readonly IHost _host;
    private readonly Dictionary<Type, ICollection> _queues;

    public RabbitMqTestHarness(IHost host, Dictionary<Type, ICollection> queues)
    {
        _host = host;
        _queues = queues;
    }

    public BlockingCollection<T> Queue<T>() =>
        _queues[typeof(T)] as BlockingCollection<T>
        ?? throw new Exception($"the collection at {typeof(T)} is not a blocking queue");

    public T Consumed<T>() =>
        Queue<T>().TryTake(out var item, TimeSpan.FromSeconds(10))
            ? item : throw new Exception($"no items in collection {typeof(T)}");

    public void Dispose()
    {
        _host.Dispose();
    }

    public class Builder
    {
        private readonly string _rabbitMqUrl;
        private readonly string _name;
        private readonly Dictionary<Type, ICollection> _queues = new();
        private readonly List<Action<IRabbitMqBusFactoryConfigurator>> _configurators = new();
        
        public Builder(string rabbitMqUrl, string name)
        {
            _rabbitMqUrl = rabbitMqUrl;
            _name = name;
        }
    
        public Builder Consume<T>() where T : class
        {
            var queueName = new Regex("[^a-zA-Z0-9-]").Replace(typeof(T).Name.Kebaberize(), "");
            var queue = new BlockingCollection<T>();
            _queues.Add(typeof(T), queue);
            _configurators.Add(o =>
            {
                o.ReceiveEndpoint(queueName, e =>
                {
                    e.Handler<T>(context =>
                    {
                        queue.Add(context.Message, context.CancellationToken);
                        return Task.CompletedTask;
                    });
                });
            });
            return this;
        }
    
        public async Task<RabbitMqTestHarness> RunAsync(CancellationToken cancellationToken = default)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddOptions<MassTransitHostOptions>()
                        .Configure(options => options.WaitUntilStarted = true);
                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();
                        x.UsingRabbitMq((c, o) =>
                        {
                            o.Host(_rabbitMqUrl);
                            
                            foreach (var configurator in _configurators)
                            {
                                configurator(o);
                            }
    
                            o.ConfigureEndpoints(c);
                        });
                    });
                })
                .Build();
            await host.StartAsync(cancellationToken);
            return new RabbitMqTestHarness(host, _queues);
        }
    }
}