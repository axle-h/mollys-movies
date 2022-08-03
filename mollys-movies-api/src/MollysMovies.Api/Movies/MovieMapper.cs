using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MollysMovies.Api.Common;
using MollysMovies.Api.Movies.Models;
using MollysMovies.Api.Movies.Requests;
using MollysMovies.Common.Movies;

namespace MollysMovies.Api.Movies;

public interface IMovieMapper
{
    MovieDto ToMovieDto(Movie movie);

    PaginatedMovieQuery ToPaginatedMovieQuery(SearchMoviesRequest query);
}

public class MovieMapper : IMovieMapper
{
    public PaginatedMovieQuery ToPaginatedMovieQuery(SearchMoviesRequest request) => new()
    {
        Text = request.Title,
        Quality = request.Quality,
        Language = request.Language,
        HasDownload = request.HasDownload,
        Downloaded = request.Downloaded,
        Genre = request.Genre,
        YearFrom = request.YearFrom,
        YearTo = request.YearTo,
        RatingFrom = request.RatingFrom,
        RatingTo = request.RatingTo,
        Page = request.Page ?? 1,
        Limit = request.Limit ?? 20, // TODO default limit?
        OrderBy = GetOrderBy(request.OrderBy, request.OrderByDescending)
    };

    public MovieDto ToMovieDto(Movie movie) => new(
        movie.ImdbCode,
        movie.Meta!.Title!,
        movie.Meta!.Language,
        movie.Meta!.Year,
        movie.Meta!.Rating,
        movie.Meta!.Description,
        movie.Meta!.YouTubeTrailerCode,
        movie.Meta!.ImageFilename,
        movie.Meta!.Genres,
        movie.Torrents.Select(ToTorrentDto).ToList(),
        movie.LocalSource is null ? null : ToLocalMovieSourceDto(movie.LocalSource),
        movie.Download is null ? null : ToMovieDownloadDto(movie.ImdbCode, movie.Download)
    );

    private static MovieDownloadDto ToMovieDownloadDto(string imdbCode, MovieDownload download) =>
        new(imdbCode,
            download.ExternalId ?? throw new ArgumentNullException(nameof(download), "external id is required"),
            download.Name ?? throw new ArgumentNullException(nameof(download), "name is required"),
            download.Statuses
                .OrderByDescending(s => s.DateCreated)
                .Select(s => s.Status as MovieDownloadStatusCode?)
                .FirstOrDefault() ?? throw new ArgumentNullException(nameof(download), "status must not be empty"));

    private static LocalMovieSourceDto ToLocalMovieSourceDto(LocalMovieSource movie) =>
        new(movie.Source ?? throw new ArgumentNullException(nameof(movie), "source is required"),
            movie.DateCreated, movie.DateScraped);

    private static TorrentDto ToTorrentDto(Torrent torrent) =>
        new(torrent.Source ?? throw new ArgumentNullException(nameof(torrent), "source is required"),
            torrent.Url ?? throw new ArgumentNullException(nameof(torrent), "url is required"),
            torrent.Hash ?? throw new ArgumentNullException(nameof(torrent), "hash is required"),
            torrent.Quality ?? throw new ArgumentNullException(nameof(torrent), "quality is required"),
            torrent.Type ?? throw new ArgumentNullException(nameof(torrent), "type is required"),
            torrent.SizeBytes);

    private static ICollection<PaginatedOrderBy<Movie>> GetOrderBy(MoviesOrderBy? orderBy, bool? descending)
    {
        PaginatedOrderBy<Movie> OrderBy(Expression<Func<Movie, object?>> property) =>
            new(property, descending ?? false);

        return (orderBy ?? MoviesOrderBy.Title) switch
        {
            MoviesOrderBy.Title => new[] {OrderBy(x => x.Meta!.Title)},
            MoviesOrderBy.Year => new[] {OrderBy(x => x.Meta!.Year), OrderBy(x => x.Meta!.Title!)},
            MoviesOrderBy.Rating => new[] {OrderBy(x => x.Meta!.Rating)},
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy))
        };
    }
}