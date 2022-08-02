using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace MollysMovies.Api.MovieImages;

public static class MovieImageExtensions
{
    public static IApplicationBuilder UseStaticMovieImages(this WebApplication app) =>
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = app.Services.GetRequiredService<IMovieImageFileProviderFactory>().Build(),
            RequestPath = "/movie-images",
            ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".jpeg"] = "image/jpeg",
                [".jpg"] = "image/jpeg",
                [".png"] = "image/png",
            }),
            ServeUnknownFileTypes = false
        });
}