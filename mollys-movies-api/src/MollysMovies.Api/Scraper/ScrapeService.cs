using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MollysMovies.Api.Scraper.Models;
using MollysMovies.Common;

namespace MollysMovies.Api.Scraper;

public interface IScrapeService
{
    Task<ICollection<ScrapeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ScrapeDto> CreateScrapeAsync(CancellationToken cancellationToken = default);
}

public class ScrapeService : IScrapeService
{
    private readonly ISystemClock _clock;
    private readonly IScrapeMapper _mapper;
    private readonly IScrapeRepository _repository;

    public ScrapeService(ISystemClock clock, IScrapeMapper mapper, IScrapeRepository repository)
    {
        _clock = clock;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<ICollection<ScrapeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var scrapes = await _repository.GetAllAsync(cancellationToken);
        return scrapes.Select(_mapper.ToScrapeDto).ToList();
    }

    public async Task<ScrapeDto> CreateScrapeAsync(CancellationToken cancellationToken = default)
    {
        var scrape = await _repository.InsertScrapeAsync(_clock.UtcNow, cancellationToken);
        return _mapper.ToScrapeDto(scrape);
    }
}