using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MolliesMovies.Common;
using MolliesMovies.Common.Data;
using MolliesMovies.Common.Exceptions;
using MolliesMovies.Movies;
using MolliesMovies.Movies.Models;
using MolliesMovies.Scraper;
using MolliesMovies.Transmission.Data;
using MolliesMovies.Transmission.Models;
using Transmission.API.RPC;
using Transmission.API.RPC.Entity;

namespace MolliesMovies.Transmission
{
    public interface ITransmissionService
    {
        Task DownloadMovieTorrentAsync(int movieId, int torrentId, CancellationToken cancellationToken = default);

        Task<LiveTransmissionStatusDto> GetLiveTransmissionStatusAsync(int movieId, int torrentId, CancellationToken cancellationToken = default);
        
        Task<TransmissionContextDto> GetActiveContextByExternalIdAsync(int externalId, CancellationToken cancellationToken = default);
        
        Task CompleteActiveContextAsync(int externalId, CancellationToken cancellationToken = default);
        Task<Paginated<TransmissionContextDto>> GetAllContextsAsync(PaginatedRequest request, CancellationToken cancellationToken = default);
    }

    public class TransmissionService : ITransmissionService
    {
        private readonly IOptions<TransmissionOptions> _options;
        private readonly IMovieService _movieService;
        private readonly IScraperService _scraperService;
        private readonly IScraperBackgroundService _scraperBackgroundService;
        private readonly ILogger<TransmissionService> _logger;
        private readonly MolliesMoviesContext _context;
        private readonly ISystemClock _clock;
        private readonly IMapper _mapper;

        public TransmissionService(
            IOptions<TransmissionOptions> options,
            IMovieService movieService,
            ILogger<TransmissionService> logger,
            MolliesMoviesContext context,
            ISystemClock clock,
            IScraperService scraperService,
            IScraperBackgroundService scraperBackgroundService,
            IMapper mapper)
        {
            _options = options;
            _movieService = movieService;
            _logger = logger;
            _context = context;
            _clock = clock;
            _scraperService = scraperService;
            _scraperBackgroundService = scraperBackgroundService;
            _mapper = mapper;
        }

        public async Task DownloadMovieTorrentAsync(int movieId, int torrentId, CancellationToken cancellationToken = default)
        {
            var options = _options.Value;
            var movie = await _movieService.GetAsync(movieId, cancellationToken);
            var torrent = movie.MovieSources
                .SelectMany(x => x.Torrents)
                .FirstOrDefault(x => x.Id == torrentId);

            if (torrent is null)
            {
                throw BadRequestException.Create($"torrent with id {torrentId} does not exist on movie with id {movieId}");
            }

            if (!(movie.LocalMovie is null))
            {
                throw BadRequestException.Create($"movie with id {movieId} is already downloaded");
            }

            if (movie.TransmissionStatus.HasValue)
            {
                throw BadRequestException.Create($"movie with id {movieId} is currently downloading");
            }

            var torrentName = $"{movie.Title} ({movie.Year})";

            var client = new Client(options.RpcUri.ToString());
            var torrents = await client.TorrentGetAsync(TorrentFields.ALL_FIELDS);
            if (torrents.Torrents.Any(x => x.Name == torrentName))
            {
                throw BadRequestException.Create($"movie with id {movieId} is currently downloading");
            }
            
            // get magnet uri
            var trs = string.Join('&', options.Trackers.Select(x => $"tr={x}"));
            var magnetUri = $"magnet:?xt=urn:btih:{torrent.Hash}&dn={Uri.EscapeDataString(torrentName)}&{trs}";

            var result = await client.TorrentAddAsync(new NewTorrent
            {
                Filename = magnetUri
            });
            
            _logger.LogInformation("added torrent {name}", result.Name);

            var context = new TransmissionContext
            {
                ExternalId = result.ID,
                Name = result.Name,
                MagnetUri = magnetUri,
                MovieId = movie.Id,
                TorrentId = torrent.Id,
                Statuses = new List<TransmissionContextStatus> { GetStatus(TransmissionStatusCode.Started) }
            };
            _context.Add(context);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("saved context for torrent {name}", result.Name);
        }

        public async Task<LiveTransmissionStatusDto> GetLiveTransmissionStatusAsync(int movieId, int torrentId, CancellationToken cancellationToken = default)
        {
            var context = await _context.TransmissionContexts()
                .FirstOrDefaultAsync(x => x.MovieId == movieId && x.TorrentId == torrentId, cancellationToken);
            if (context is null)
            {
                throw EntityNotFoundException.Of<TransmissionContext>(new { MovieId = movieId, TorrentId = torrentId });
            }

            var status = context.GetStatus();
            if (status != TransmissionStatusCode.Started)
            {
                return LiveTransmissionStatusDto.GetComplete(context.Name);
            }
            
            // Get live status.
            var client = new Client(_options.Value.RpcUri.ToString());
            var torrents = await client.TorrentGetAsync(TorrentFields.ALL_FIELDS, context.ExternalId);
            var torrent = torrents.Torrents.FirstOrDefault();
            if (torrent is null)
            {
                return LiveTransmissionStatusDto.GetComplete(context.Name);
            }

            return new LiveTransmissionStatusDto
            {
                Name = context.Name,
                Complete = !(torrent.PercentDone < 1.0),
                Stalled = torrent.IsStalled,
                Eta = torrent.ETA <= 0 ? null : torrent.ETA as int?,
                PercentComplete = torrent.PercentDone,
            };
        }

        public async Task<TransmissionContextDto> GetActiveContextByExternalIdAsync(int externalId, CancellationToken cancellationToken = default)
        {
            var context = await _context.TransmissionContexts()
                .FirstOrDefaultAsync(x => x.ExternalId == externalId
                                          && x.Statuses.Any()
                                          && x.Statuses.OrderBy(s => s.DateCreated).Last().Status == TransmissionStatusCode.Started,
                    cancellationToken);
            if (context is null)
            {
                throw EntityNotFoundException.Of<TransmissionContext>(new { ExternalId = externalId });
            }

            return _mapper.Map<TransmissionContextDto>(context);
        }

        public async Task CompleteActiveContextAsync(int externalId, CancellationToken cancellationToken = default)
        {
            var context = await GetActiveContextByExternalIdAsync(externalId, cancellationToken);

            await SetStatusAsync(TransmissionStatusCode.Downloaded, context.Id, cancellationToken);

            _scraperBackgroundService.AddScrapeForLocalMovieJob(context.MovieId);

            await SetStatusAsync(TransmissionStatusCode.Complete, context.Id, CancellationToken.None);
            _logger.LogInformation("initiated scrape for downloaded movie {name}", context.Name);
        }

        public async Task<Paginated<TransmissionContextDto>> GetAllContextsAsync(PaginatedRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TransmissionContext>();
            var count = await query.CountAsync(cancellationToken);

            var page = request.Page ?? 1;
            var limit = request.Limit ?? 20;
            var data = count > 0
                ? await query
                    .OrderByDescending(x => x.Id)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ProjectTo<TransmissionContextDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken)
                : new List<TransmissionContextDto>();

            return new Paginated<TransmissionContextDto>
            {
                Page = page,
                Limit = limit,
                Count = count,
                Data = data
            };
        }

        private async Task SetStatusAsync(TransmissionStatusCode code, int contextId, CancellationToken cancellationToken = default)
        {
            var nextStatus = GetStatus(code, contextId);
            _context.Add(nextStatus);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private TransmissionContextStatus GetStatus(TransmissionStatusCode code, int? contextId = null) =>
            new TransmissionContextStatus
            {
                Status = code,
                DateCreated = _clock.UtcNow,
                TransmissionContextId = contextId.GetValueOrDefault()
            };
    }
}