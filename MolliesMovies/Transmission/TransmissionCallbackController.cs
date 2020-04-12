using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MolliesMovies.Transmission.Models;

namespace MolliesMovies.Transmission
{
    [Route("/api/transmission-callback")]
    public class TransmissionCallbackController : ControllerBase
    {
        private readonly ITransmissionService _service;

        public TransmissionCallbackController(ITransmissionService service)
        {
            _service = service;
        }
        
        [HttpGet("{externalId:int}")]
        public async Task<TransmissionContextDto> GetContextByExternalId(int externalId, CancellationToken cancellationToken = default) =>
            await _service.GetContextByExternalIdAsync(externalId, cancellationToken);
        
        [HttpPost("{externalId:int}")]
        public async Task CompleteCallback(int externalId, CancellationToken cancellationToken = default) =>
            await _service.CompleteCallbackAsync(externalId, cancellationToken);
    }
}