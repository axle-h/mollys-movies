using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MollysMovies.Api.Common;
using MollysMovies.Api.Common.Exceptions;
using MollysMovies.Api.Movies;
using MollysMovies.Api.Movies.Models;
using MollysMovies.Api.Movies.Requests;
using MollysMovies.Common.Movies;

namespace MollysMovies.Api.Transmission;

public interface ITransmissionDownloadService
{
    Task<MovieDownloadDto> GetActiveAsync(string externalId, CancellationToken cancellationToken = default);

    Task<PaginatedData<MovieDownloadDto>> SearchAsync(PaginatedRequest request,
        CancellationToken cancellationToken = default);
}

public class TransmissionDownloadService : ITransmissionDownloadService
{
    private readonly IMovieDownloadService _movieDownloadService;
    private readonly IMovieService _movieService;

    public TransmissionDownloadService(IMovieDownloadService movieDownloadService, IMovieService movieService)
    {
        _movieDownloadService = movieDownloadService;
        _movieService = movieService;
    }

    public async Task<MovieDownloadDto> GetActiveAsync(string externalId, CancellationToken cancellationToken = default)
    {
        var movie = await _movieDownloadService.GetMovieByDownloadExternalIdAsync(externalId, cancellationToken);
        if (movie.Download?.Status != MovieDownloadStatusCode.Started)
        {
            throw EntityNotFoundException.Of<MovieDownload>(new {ExternalId = externalId},
                "context is not active");
        }

        return movie.Download;
    }

    public async Task<PaginatedData<MovieDownloadDto>> SearchAsync(PaginatedRequest request,
        CancellationToken cancellationToken = default)
    {
        var movieRequest = new SearchMoviesRequest
        {
            HasDownload = true,
            Page = request.Page,
            Limit = request.Limit
        };
        var movies = await _movieService.SearchAsync(movieRequest, cancellationToken);
        return new PaginatedData<MovieDownloadDto>
        {
            Page = movies.Page,
            Limit = movies.Limit,
            Count = movies.Count,
            Data = movies.Data.Select(x => x.Download!).ToList()
        };
    }
}