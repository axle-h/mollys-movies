using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MollysMovies.Api.Common;
using MollysMovies.Api.Common.Exceptions;
using MollysMovies.Api.Movies.Models;
using MollysMovies.Api.Movies.Requests;
using MollysMovies.Common.Movies;

namespace MollysMovies.Api.Movies;

public interface IMovieService
{
    Task<MovieDto> GetAsync(string imdbCode, CancellationToken cancellationToken = default);

    Task<PaginatedData<MovieDto>> SearchAsync(
        SearchMoviesRequest request,
        CancellationToken cancellationToken = default);

    Task<ICollection<string>> GetAllGenresAsync(CancellationToken cancellationToken = default);
}

public class MovieService : IMovieService
{
    private readonly IMovieMapper _mapper;
    private readonly IMovieRepository _repository;

    public MovieService(IMovieMapper mapper, IMovieRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<MovieDto> GetAsync(string imdbCode, CancellationToken cancellationToken = default)
    {
        var movie = await _repository.GetByImdbCodeAsync(imdbCode, cancellationToken)
                    ?? throw EntityNotFoundException.Of<Movie>(new {ImdbCode = imdbCode});
        return _mapper.ToMovieDto(movie);
    }

    public async Task<PaginatedData<MovieDto>> SearchAsync(
        SearchMoviesRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _mapper.ToPaginatedMovieQuery(request);
        var result = await _repository.SearchAsync(query, cancellationToken);
        return new PaginatedData<MovieDto>
        {
            Page = result.Page,
            Limit = result.Limit,
            Count = result.Count,
            Data = result.Data.Select(_mapper.ToMovieDto).ToList()
        };
    }

    public async Task<ICollection<string>> GetAllGenresAsync(CancellationToken cancellationToken = default) =>
        await _repository.GetAllGenresAsync(cancellationToken);
}