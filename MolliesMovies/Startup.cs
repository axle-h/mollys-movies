using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MihaZupan;
using MolliesMovies.Common;
using MolliesMovies.Common.Data;
using MolliesMovies.Common.Exceptions;
using MolliesMovies.Common.Routing;
using MolliesMovies.Movies;
using MolliesMovies.Movies.Data;
using MolliesMovies.Scraper;
using MolliesMovies.Scraper.Plex;
using MolliesMovies.Scraper.Yts;
using MolliesMovies.Transmission;

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
                .UseMySql(mysqlConnectionString, ServerVersion.AutoDetect(mysqlConnectionString)));

            if (_env.IsEnvironment(MigrationEnvironment))
            {
                services.AddHostedService<MigrationHostedService>();
                return;
            }
            
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        var origins = _configuration.GetSection("CorsOrigins").GetChildren()
                            .Select(x => x.Get<string>())
                            .ToArray();
                        builder.WithOrigins(origins);
                    });
            });
            
            services.AddSpaStaticFiles(configuration => configuration.RootPath = "wwwroot");
            
            services.AddControllers(o =>
                {
                    o.Filters.Add<ApiExceptionFilter>();
                    o.Conventions.Add(new RouteTokenTransformerConvention(
                        new SlugifyParameterTransformer()));
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                })
                .AddFluentValidation(c =>
                {
                    c.RegisterValidatorsFromAssemblyContaining<Startup>();
                    c.ImplicitlyValidateChildProperties = true;
                });
            
            services.AddRouting(o =>
            {
                o.LowercaseUrls = true;
            });
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mollies Movies", Version = "v1" });
            });
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(RouteConstants.PublicApi(1), new OpenApiInfo { Title = "Public Mollies Movies API", Version = "v1" });
                c.CustomSchemaIds(t => t.DtoSafeFriendlyId());
                c.CustomOperationIds(ad => ad.GetOperationId());
                c.DocInclusionPredicate((name, api) => api.IsApi(name));
            });

            services.Configure<MovieOptions>(_configuration.GetSection("Movie"));
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
            
            var ytsClientBuilder = services.AddHttpClient<IYtsClient, YtsClient>(c => { c.BaseAddress = ytsUri; });
            
            var proxyUrl = _configuration.GetConnectionString("proxy");
            if (!string.IsNullOrEmpty(proxyUrl))
            {
                if (!Uri.TryCreate(proxyUrl, UriKind.Absolute, out var uri))
                {
                    throw new Exception("proxy url is invalid");
                }

                ytsClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {Proxy = new HttpToSocks5Proxy(uri.Host, uri.Port)});                
            }

            var plexUri = GetApiUrl("plex");
            services.AddHttpClient<IPlexApiClient, PlexApiClient>(c => { c.BaseAddress = plexUri; });
        }

        private Uri GetApiUrl(string name)
        {
            if (!Uri.TryCreate(_configuration.GetConnectionString(name), UriKind.Absolute, out var uri))
            {
                throw new Exception($"valid {name} url is required");
            }

            return uri;
        }

        public void Configure(IApplicationBuilder app, IServiceProvider provider, IOptions<MovieOptions> options)
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
            app.UseSwaggerUI(c => c.PublicApi(1));

            app.UseRouting();
            app.UseCors();
            
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(options.Value.ImagePath),
                RequestPath = "/movie-images"
            });
            
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            
            app.UseSpaStaticFiles();
            app.UseSpa(o =>
            {
                if (_env.IsDevelopment())
                {
                    o.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}