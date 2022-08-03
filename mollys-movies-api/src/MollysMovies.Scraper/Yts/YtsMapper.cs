using System.Collections.Generic;
using System.Linq;
using MollysMovies.Scraper.Models;
using MollysMovies.Scraper.Yts.Models;

namespace MollysMovies.Scraper.Yts;

public interface IYtsMapper
{
    CreateMovieRequest ToCreateMovieRequest(YtsMovieSummary movie);

    CreateTorrentRequest ToCreateTorrentRequest(YtsTorrent torrent);
}

public class YtsMapper : IYtsMapper
{
    public CreateTorrentRequest ToCreateTorrentRequest(YtsTorrent torrent) =>
        new(
            torrent.Url.AbsoluteUri,
            torrent.Hash,
            torrent.Quality,
            torrent.Type,
            torrent.SizeBytes
        );

    public CreateMovieRequest ToCreateMovieRequest(YtsMovieSummary movie) =>
        new(
            movie.ImdbCode,
            movie.Title,
            movie.Language,
            movie.Year,
            movie.Rating,
            NullIfEmpty(movie.DescriptionFull),
            movie.Genres ?? new List<string>(),
            NullIfEmpty(movie.YtTrailerCode),
            movie.LargeCoverImage.PathAndQuery,
            movie.Url.PathAndQuery,
            movie.Id.ToString(),
            movie.DateUploaded,
            movie.Torrents?.Select(ToCreateTorrentRequest).ToList() ?? new List<CreateTorrentRequest>()
        );

    private static string? NullIfEmpty(string? s) => string.IsNullOrEmpty(s) ? null : s;
}