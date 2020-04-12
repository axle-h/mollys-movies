using System;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MolliesMovies.Common;
using MolliesMovies.Common.Data;
using MolliesMovies.Common.Exceptions;
using MolliesMovies.Movies;
using MolliesMovies.Movies.Data;
using MolliesMovies.Scraper;
using MolliesMovies.Scraper.Data;
using MolliesMovies.Scraper.Plex;
using MolliesMovies.Scraper.Yts;
using MolliesMovies.Transmission;
using Polly;
using Polly.Extensions.Http;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace MolliesMovies
{
    public class Startup
    {
        private const string MigrationEnvironment = "Migration";
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var mysqlConnectionString = _configuration.GetConnectionString("mysql");
            if (string.IsNullOrEmpty(mysqlConnectionString))
            {
                throw new Exception("mysql connection string is required");
            }
            services.AddDbContext<MolliesMoviesContext>(o => o
                .UseMySql(mysqlConnectionString, mo => mo.ServerVersion(new Version(8, 0, 19), ServerType.MySql)));

            if (_env.IsEnvironment(MigrationEnvironment))
            {
                services.AddHostedService<MigrationHostedService>();
                return;
            }
            
            services.AddControllers(o =>
                {
                    o.Filters.Add<ApiExceptionFilter>();
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mollies Movies", Version = "v1" });
            });

            services.Configure<ScraperOptions>(_configuration.GetSection("Scraper"));
            services.Configure<TransmissionOptions>(_configuration.GetSection("Transmission"));
            services.PostConfigure<TransmissionOptions>(o => o.RpcUri = GetApiUrl("transmission"));
            
            services.AddSingleton<ScraperBackgroundService>();
            services.AddSingleton<IHostedService, ScraperBackgroundService>(p => p.GetRequiredService<ScraperBackgroundService>());
            services.AddSingleton<IScraperBackgroundService, ScraperBackgroundService>(p => p.GetRequiredService<ScraperBackgroundService>());

            services.AddTransient<IScraperService, ScraperService>();
            services.AddTransient<IScraperInternalService, ScraperService>();
            services.AddTransient<IScraper, YtsScraper>();
            services.AddTransient<IScraper, PlexScraper>();
            
            services.AddTransient<IMovieService, MovieService>();
            
            services.AddTransient<ITransmissionService, TransmissionService>();

            services.AddSingleton<ISystemClock, SystemClock>();
            services.AddAutoMapper(typeof(Startup));

            var ytsUri = GetApiUrl("yts");
            services.AddHttpClient<IYtsClient, YtsClient>(c => { c.BaseAddress = ytsUri; })
                .AddPolicyHandler(
                    (provider, request) => HttpPolicyExtensions.HandleTransientHttpError()
                        .WaitAndRetryAsync(6, (retry, context) => TimeSpan.FromSeconds(Math.Pow(2, retry)),
                            (outcome, timespan, retryAttempt, context) =>
                            {
                                provider.GetService<ILogger<YtsClient>>()?
                                    .LogWarning("delaying for {delay}ms, then making retry {retry}.",
                                        timespan.TotalMilliseconds, retryAttempt);
                            }
                        ));

            var plexUri = GetApiUrl("plex");
            services.AddHttpClient<IPlexApiClient, PlexApiClient>(c => { c.BaseAddress = plexUri; });
        }

        private Uri GetApiUrl(string name)
        {
            if (!Uri.TryCreate(_configuration.GetConnectionString(name), UriKind.Absolute,
                out var uri))
            {
                throw new Exception($"valid {name} url is required");
            }

            return uri;
        }

        public void Configure(IApplicationBuilder app, IServiceProvider provider)
        {
            if (_env.IsEnvironment(MigrationEnvironment))
            {
                return;
            }
            
            provider.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mollies Movies V1"));

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}