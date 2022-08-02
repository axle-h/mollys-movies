using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MollysMovies.Common.Health;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

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

    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        services.AddOptions<MongoInitOptions>().BindConfiguration("Mongo");

        services
            .AddSingleton<IMongoClient>(p =>
            {
                var url = p.GetRequiredService<IConfiguration>().GetMongoUrl();
                return new MongoClient(MongoClientSettings.FromUrl(url));
            })
            .AddSingleton<IMongoDatabase>(p =>
            {
                var url = p.GetRequiredService<IConfiguration>().GetMongoUrl();
                return p.GetRequiredService<IMongoClient>().GetDatabase(url.DatabaseName);
            })
            .AddSingleton<IMongoInitService, MongoInitService>()
            .AddHostedService(p => p.GetRequiredService<IMongoInitService>());

        services.AddHealthChecks()
            .AddProbe<MongoHealthCheck>("mongo");

        return services;
    }
}