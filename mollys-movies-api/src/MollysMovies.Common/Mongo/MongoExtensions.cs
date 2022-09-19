using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MollysMovies.Common.Health;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace MollysMovies.Common.Mongo;

public static class MongoExtensions
{
    static MongoExtensions()
    {
        BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
    }

    public static MongoUrl GetMongoUrl(this IConfiguration configuration)
    {
        var mongoConnectionString = new MongoUrl(configuration.GetConnectionString("mongo"));
        if (string.IsNullOrEmpty(mongoConnectionString.DatabaseName))
        {
            throw new Exception("Must provide a database name with the mongo connection string");
        }
        return mongoConnectionString;
    }

    /// <summary>
    /// Adds the MongoDB database to the specified service collection. 
    /// </summary>
    /// <param name="services">The service collection to mutate.</param>
    /// <param name="mongoUrl">Optional mongo URL, this is taken from configured connections strings otherwise.</param>
    /// <returns>The mutated service collection</returns>
    public static IServiceCollection AddMongo(this IServiceCollection services, MongoUrl? mongoUrl = null)
    {
        services.AddOptions<MongoInitOptions>().BindConfiguration("Mongo");

        services
            .AddSingleton<IMongoClient>(p =>
            {
                var url = mongoUrl ?? p.GetRequiredService<IConfiguration>().GetMongoUrl();
                var mongoClientSettings = MongoClientSettings.FromUrl(url);
                var logger = p.GetService<ILoggerFactory>()?.CreateLogger("mongo");
                if (logger is not null && logger.IsEnabled(LogLevel.Debug))
                {
                    mongoClientSettings.ClusterConfigurator = cb => {
                        cb.Subscribe<CommandStartedEvent>(e => {
                            logger.LogDebug("{CommandName} - {Command}", e.CommandName, e.Command.ToJson());
                        });
                    };
                }
                
                return new MongoClient(mongoClientSettings);
            })
            .AddSingleton<IMongoDatabase>(p =>
            {
                var url = mongoUrl ?? p.GetRequiredService<IConfiguration>().GetMongoUrl();
                return p.GetRequiredService<IMongoClient>().GetDatabase(url.DatabaseName);
            })
            .AddSingleton<IMongoInitService, MongoInitService>()
            .AddHostedService(p => p.GetRequiredService<IMongoInitService>());

        services.AddHealthChecks()
            .AddProbe<MongoHealthCheck>("mongo");

        return services;
    }
}