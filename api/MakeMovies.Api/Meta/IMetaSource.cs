namespace MakeMovies.Api.Meta;

public record MovieMeta(string ImdbCode, string? ImageUrl);

public interface IMetaSource
{
    int Priority { get; }
    
    Task<MovieMeta?> GetByImdbCodeAsync(string imdbCode, CancellationToken cancellationToken = default);
}