using System;
using System.IO.Abstractions;
using FluentValidation;
using MollysMovies.Common.Validation;

namespace MollysMovies.Scraper;

public class ScraperOptions
{
    public string ImagePath { get; set; } = string.Empty;
    
    public string MovieLibraryPath { get; set; } = string.Empty;
    
    public string DownloadsPath { get; set; } = string.Empty;

    public int ImageScraperPageSize { get; set; }

    public TimeSpan RemoteScrapeDelay { get; set; } = TimeSpan.Zero;

    public TimeSpan LocalUpdateMovieDelay { get; set; } = TimeSpan.Zero;

    public PlexOptions? Plex { get; set; }

    public YtsOptions? Yts { get; set; }
}

public class PlexOptions
{
    public string Token { get; set; } = string.Empty;
}

public class YtsOptions
{
    public TimeSpan RetryDelay { get; set; } = TimeSpan.Zero;

    public int Limit { get; set; } = 50;
}

public class ScraperOptionsValidator : AbstractValidator<ScraperOptions>
{
    public ScraperOptionsValidator(IFileSystem fileSystem)
    {
        RuleFor(x => x.ImagePath)
            .NotNull().NotEmpty()
            .DirectoryExists(fileSystem)
            .WriteableDirectory(fileSystem);
        RuleFor(x => x.MovieLibraryPath)
            .NotNull().NotEmpty()
            .DirectoryExists(fileSystem)
            .WriteableDirectory(fileSystem);
        RuleFor(x => x.DownloadsPath)
            .NotNull().NotEmpty()
            .DirectoryExists(fileSystem)
            .WriteableDirectory(fileSystem);
        RuleFor(x => x.ImageScraperPageSize).GreaterThan(0);
        RuleFor(x => x.RemoteScrapeDelay).GreaterThanOrEqualTo(TimeSpan.Zero);
        RuleFor(x => x.LocalUpdateMovieDelay).GreaterThanOrEqualTo(TimeSpan.Zero);
        RuleFor(x => x.Plex).NotNull().SetValidator(new PlexOptionsValidator());
        RuleFor(x => x.Yts).NotNull().SetValidator(new YtsOptionsValidator());
    }

    private class PlexOptionsValidator : AbstractValidator<PlexOptions?>
    {
        public PlexOptionsValidator()
        {
            RuleFor(x => x!.Token).NotNull().NotEmpty();
        }
    }

    private class YtsOptionsValidator : AbstractValidator<YtsOptions?>
    {
        public YtsOptionsValidator()
        {
            RuleFor(x => x!.RetryDelay).GreaterThanOrEqualTo(TimeSpan.Zero);
            RuleFor(x => x!.Limit).GreaterThan(0);
        }
    }
}