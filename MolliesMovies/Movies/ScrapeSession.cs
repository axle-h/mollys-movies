using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using MolliesMovies.Common;
using MolliesMovies.Common.Data;
using MolliesMovies.Movies.Data;
using MolliesMovies.Movies.Models;
using MolliesMovies.Movies.Requests;
using MolliesMovies.Scraper.Data;

namespace MolliesMovies.Movies
{
    public interface IScrapeSession
    {
        DateTime? ScrapeFrom { get; }
        
        List<string> MovieImdbCodes { get; }
        
        List<string> LocalImdbCodes { get; }

        Task CreateMoviesAsync(ICollection<CreateMovieRequest> requests);

        Task CreateLocalMoviesAsync(ICollection<CreateLocalMovieRequest> requests);

        Task CreateMovieImageAsync(CreateMovieImageRequest request, CancellationToken cancellationToken = default);
        void CreateMovieImageDirectory();
        Task<bool> AssertMovieImageAsync(string imdbCode, CancellationToken cancellationToken = default);
    }

    public class ScrapeSession : IScrapeSession
    {
        private readonly string _source;
        private readonly ScraperType _type;
        private readonly MolliesMoviesContext _context;
        private readonly CancellationToken _cancellationToken;
        private readonly DateTime _scrapeDate;
        private readonly MovieOptions _options;

        public ScrapeSession(
            string source,
            ScraperType type,
            DateTime? scrapeFrom,
            List<string> movieImdbCodes,
            List<string> localImdbCodes,
            MolliesMoviesContext context,
            ISystemClock clock,
            CancellationToken cancellationToken,
            MovieOptions options)
        {
            ScrapeFrom = scrapeFrom;
            _source = source;
            _type = type;
            MovieImdbCodes = movieImdbCodes;
            LocalImdbCodes = localImdbCodes;
            _context = context;
            _scrapeDate = clock.UtcNow;
            _cancellationToken = cancellationToken;
            _options = options;
        }

        public DateTime? ScrapeFrom { get; }

        public List<string> MovieImdbCodes { get; }

        public List<string> LocalImdbCodes { get; }

        public async Task CreateLocalMoviesAsync(ICollection<CreateLocalMovieRequest> requests)
        {
            if (_type != ScraperType.Local)
            {
                throw new InvalidOperationException($"scrape session of type {_type} cannot create local movies");
            }

            var toAdd = requests.Where(x => !LocalImdbCodes.Contains(x.ImdbCode))
                .Select(x => new LocalMovie
                {
                    Source = _source,
                    ImdbCode = x.ImdbCode.Trim().ToLower(),
                    Title = x.Title,
                    Year = x.Year,
                    DateCreated = x.DateCreated,
                    DateScraped = _scrapeDate,
                    ThumbPath = x.ThumbPath
                })
                .ToList();

            if (!toAdd.Any())
            {
                return;
            }

            LocalImdbCodes.AddRange(toAdd.Select(x => x.ImdbCode));
            _context.AddRange(toAdd);

            var downloadedMovies = toAdd
                .Join(MovieImdbCodes, lm => lm.ImdbCode, x => x, (lm, x) => new DownloadedMovie
                {
                    MovieImdbCode = x,
                    LocalMovieImdbCode = x
                });
            _context.AddRange(downloadedMovies);

            await _context.SaveChangesAsync(_cancellationToken);
        }

        public async Task CreateMoviesAsync(ICollection<CreateMovieRequest> requests)
        {
            if (_type != ScraperType.Torrent)
            {
                throw new InvalidOperationException($"scrape session of type {_type} cannot create torrent movies");
            }

            static string Clean(string s) => s.Trim().ToLower();
            var requestedGenres = requests.SelectMany(x => x.Genres).Distinct().ToList();
            var genres = await _context.AssertGenresAsync(requestedGenres, _cancellationToken);
            foreach (var request in requests)
            {
                var source = new MovieSource
                {
                    Source = _source,
                    DateCreated = request.DateCreated,
                    DateScraped = _scrapeDate,
                    SourceId = request.SourceId,
                    SourceUrl = request.SourceUrl,
                    SourceCoverImageUrl = request.SourceCoverImageUrl,
                    Torrents = request.Torrents
                        ?.Select(x => new Torrent
                        {
                            Url = x.Url,
                            Quality = x.Quality,
                            Type = x.Type,
                            Hash = x.Hash,
                            SizeBytes = x.SizeBytes
                        })
                        .ToList()
                };

                var imdbCode = request.ImdbCode.Trim().ToLower();
                if (MovieImdbCodes.Contains(imdbCode))
                {
                    // Add source to existing movie.
                    var addedMovie = _context.ChangeTracker.Entries<Movie>().FirstOrDefault(x =>
                        x.State == EntityState.Added && x.Entity.ImdbCode == imdbCode);
                    if (addedMovie is null)
                    {
                        // movie is already in db
                        var existingMovie = await _context.GetMovieByImdbCodeAsync(imdbCode, _cancellationToken);
                        source.MovieId = existingMovie.Id;
                        _context.Add(source);
                    }
                    else
                    {
                        // movie is being added within this batch
                        addedMovie.Entity.MovieSources.Add(source);
                    }
                }
                else
                {
                    var movie = new Movie
                    {
                        Description = request.Description,
                        Language = request.Language,
                        Rating = Math.Min(Math.Max(Math.Round(request.Rating, 1), 0), 10),
                        Title = request.Title,
                        Year = request.Year,
                        ImdbCode = imdbCode,
                        MetaSource = _source,
                        YouTubeTrailerCode = request.YouTubeTrailerCode,
                        MovieGenres = request.Genres
                            ?.Join(genres, Clean, g => Clean(g.Name), (s, genre) => genre)
                            .Select(x => new MovieGenre {GenreId = x.Id})
                            .ToList(),
                        MovieSources = new List<MovieSource> {source}
                    };
                    _context.Add(movie);
                    MovieImdbCodes.Add(imdbCode);
                    if (LocalImdbCodes.Contains(imdbCode))
                    {
                        _context.Add(new DownloadedMovie
                        {
                            MovieImdbCode = imdbCode,
                            LocalMovieImdbCode = imdbCode
                        });
                    }
                }
            }

            await _context.SaveChangesAsync(_cancellationToken);
        }

        public void CreateMovieImageDirectory() => Directory.CreateDirectory(_options.ImagePath);

        public async Task CreateMovieImageAsync(CreateMovieImageRequest request, CancellationToken cancellationToken = default)
        {
            var file = request.ImdbCode + request.ContentType.Split('/').Last() switch
            {
                "jpeg" => ".jpg",
                "jpg" => ".jpg",
                "png" => ".png",
                _ => throw new ArgumentException($"unknown image mime type {request.ContentType}")
            };
            await File.WriteAllBytesAsync(Path.Combine(_options.ImagePath, file), request.Content, cancellationToken);

            await UpdateImageFilenameAsync(request.ImdbCode, file, cancellationToken);
        }

        public async Task<bool> AssertMovieImageAsync(string imdbCode, CancellationToken cancellationToken = default)
        {
            var file = Directory.EnumerateFiles(_options.ImagePath, imdbCode + ".*").FirstOrDefault();
            if (file is null)
            {
                return false;
            }

            await UpdateImageFilenameAsync(imdbCode, Path.GetFileName(file), cancellationToken);
            return true;
        }

        private async Task UpdateImageFilenameAsync(string imdbCode, string file, CancellationToken cancellationToken)
        {
            var movie = await _context.GetMovieByImdbCodeAsync(imdbCode, cancellationToken);
            movie.ImageFilename = file;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}