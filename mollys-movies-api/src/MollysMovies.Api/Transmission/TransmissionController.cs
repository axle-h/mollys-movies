using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MollysMovies.Api.Common;
using MollysMovies.Api.Common.Routing;
using MollysMovies.Api.Movies.Models;

namespace MollysMovies.Api.Transmission;

[PublicApiRoute("transmission")]
public class TransmissionController : ControllerBase
{
    private readonly ITransmissionDownloadService _service;

    public TransmissionController(ITransmissionDownloadService service)
    {
        _service = service;
    }

    [HttpGet("{externalId}")]
    public async Task<MovieDownloadDto> GetDownloadByExternalId(string externalId,
        CancellationToken cancellationToken = default) =>
        await _service.GetActiveAsync(externalId, cancellationToken);

    [HttpGet]
    public async Task<PaginatedData<MovieDownloadDto>> GetAllDownloads(PaginatedRequest request,
        CancellationToken cancellationToken = default) =>
        await _service.SearchAsync(request, cancellationToken);
}