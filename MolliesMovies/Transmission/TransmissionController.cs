using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MolliesMovies.Common;
using MolliesMovies.Common.Routing;
using MolliesMovies.Transmission.Models;

namespace MolliesMovies.Transmission
{
    [PublicApiRoute]
    public class TransmissionController : ControllerBase
    {
        private readonly ITransmissionService _service;

        public TransmissionController(ITransmissionService service)
        {
            _service = service;
        }
        
        [HttpGet("{externalId:int}")]
        public async Task<TransmissionContextDto> GetContextByExternalId(int externalId, CancellationToken cancellationToken = default) =>
            await _service.GetActiveContextByExternalIdAsync(externalId, cancellationToken);
        
        [HttpPost("{externalId:int}")]
        public async Task CompleteCallback(int externalId, CancellationToken cancellationToken = default) =>
            await _service.CompleteActiveContextAsync(externalId, cancellationToken);

        [HttpGet]
        public async Task<Paginated<TransmissionContextDto>> GetAllContexts(PaginatedRequest request, CancellationToken cancellationToken = default) =>
            await _service.GetAllContextsAsync(request, cancellationToken);
    }
}