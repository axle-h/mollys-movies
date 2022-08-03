using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MollysMovies.Api.Common.Exceptions;
using MollysMovies.Api.Movies.Models;
using MollysMovies.Api.Movies.Requests;
using MollysMovies.Common;
using MollysMovies.Common.Movies;

namespace MollysMovies.Api.Movies;

public interface IMovieDownloadService
{
    Task SetStatusAsync(string imdbCode, MovieDownloadStatusCode status,
        CancellationToken cancellationToken = default);

    Task SetDownloadAsync(SetDownloadRequest request, CancellationToken cancellationToken = default);

    Task<MovieDto> GetMovieByDownloadExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
}

public class MovieDownloadService : IMovieDownloadService
{
    private readonly ISystemClock _clock;
    private readonly IMovieMapper _mapper;
    private readonly IMovieRepository _repository;

    public MovieDownloadService(ISystemClock clock, IMovieRepository repository, IMovieMapper mapper)
    {
        _clock = clock;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MovieDto> GetMovieByDownloadExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        var movie = await _repository.GetByExternalDownloadIdAsync(externalId, cancellationToken)
                    ?? throw EntityNotFoundException.Of<Movie>(new {ExternalDownloadId = externalId});
        return _mapper.ToMovieDto(movie);
    }

    public async Task SetStatusAsync(string imdbCode, MovieDownloadStatusCode status,
        CancellationToken cancellationToken = default)
    {
        await _repository.AddDownloadStatus(imdbCode, GetStatus(status), cancellationToken);
    }

    public async Task SetDownloadAsync(SetDownloadRequest request,
        CancellationToken cancellationToken = default)
    {
        var download = new MovieDownload
        {
            ExternalId = request.ExternalId,
            Name = request.Name,
            MagnetUri = request.MagnetUri,
            Statuses = new List<MovieDownloadStatus> {GetStatus(MovieDownloadStatusCode.Started)},
            Quality = request.Quality,
            Source = request.Source,
            Type = request.Type
        };
        await _repository.ReplaceDownload(request.ImdbCode, download, cancellationToken);
    }

    private MovieDownloadStatus GetStatus(MovieDownloadStatusCode code) =>
        new() {Status = code, DateCreated = _clock.UtcNow};
}