namespace MakeMovies.Api.Library;

public record LibraryMovie(string ImdbCode);

public interface ILibrarySource
{
    Task<ICollection<LibraryMovie>> ListAllAsync(CancellationToken cancellationToken = default);
    
    Task UpdateLibraryAsync(CancellationToken cancellationToken = default);
}