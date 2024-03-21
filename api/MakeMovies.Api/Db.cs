using System.Collections;
using System.Collections.Concurrent;
using System.IO.Abstractions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MakeMovies.Api.Downloads;
using MakeMovies.Api.Movies;
using MakeMovies.Api.Scrapes;
using Microsoft.Extensions.Options;

namespace MakeMovies.Api;

public class Db : BackgroundService
{
    private readonly IFileSystem _fileSystem;
    private readonly string _dbPath;
    private readonly ILogger<Db> _logger;
    private readonly DbCollection<Movie> _movies = new([]);
    private readonly DbCollection<Scrape> _scrapes = new([]);
    private readonly DbCollection<Download> _downloads = new([]);
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public Db(IFileSystem fileSystem, IOptions<DbOptions> options, ILogger<Db> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;

        _dbPath = options.Value.Path ?? _fileSystem.Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "make-movies");
        
        _fileSystem.Directory.CreateDirectory(_dbPath);
    }

    public IDbCollection<Movie> Movies => _movies;
    public IDbCollection<Scrape> Scrapes => _scrapes;
    public IDbCollection<Download> Downloads => _downloads;
    
    private IEnumerable<IDbCollection> Collections => [_movies, _scrapes, _downloads];
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // First of all read the db from disc
        await TryReadDbAsync(stoppingToken);
        
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            // Then periodically flush the db to disc when it is dirty
            while (
                !stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                stoppingToken.ThrowIfCancellationRequested();
                await TryWriteDirtyDbAsync(CancellationToken.None);
            }
        }
        catch (OperationCanceledException)
        {
            // Ensures a dirty db is flushed to disc when cancelled between ticks
            await TryWriteDirtyDbAsync(CancellationToken.None);
        }
    }

    private async Task TryReadDbAsync(CancellationToken cancellationToken = default)
    {
        foreach (var collection in Collections)
        {
            await collection.TryReadAsync(_fileSystem, _dbPath, _logger, cancellationToken);
        }
    }
    
    private async Task TryWriteDirtyDbAsync(CancellationToken cancellationToken = default)
    {
        foreach (var collection in Collections)
        {
            await collection.TryWriteAsync(_fileSystem, _dbPath, _logger, cancellationToken);
        }
    }

    private interface IDbCollection
    {
        Task TryReadAsync(IFileSystem fileSystem, string dbPath, ILogger logger,
            CancellationToken cancellationToken = default);

        Task TryWriteAsync(IFileSystem fileSystem, string dbPath, ILogger logger,
            CancellationToken cancellationToken = default);
    }

    public interface IDbCollection<T> : IEnumerable<T> where T : IEquatable<T>
    {
        int Count { get; }

        T? Get(string id);

        Task UpsertAsync(string id, T? value, CancellationToken cancellationToken = default);
    }
    
    public class DbCollection<T> : IDbCollection, IDbCollection<T>
        where T : IEquatable<T>
    {
        private readonly PropertyInfo _getId = typeof(T).GetProperty("Id")
                                               ?? throw new Exception($"type {typeof(T).Name} has no Id property");
        private readonly SemaphoreSlim _lock = new(1);
        private readonly ConcurrentDictionary<string, T> _data;
        private readonly string _filename = $"{typeof(T).Name.ToLower()}s.json";
        private bool _dirty;

        public DbCollection(Dictionary<string, T> data)
        {
            _data = new ConcurrentDictionary<string, T>(data);
        }
        
        public async Task TryReadAsync(IFileSystem fileSystem, string dbPath, ILogger logger, CancellationToken cancellationToken = default)
        {
            var collectionDbPath = fileSystem.Path.Join(dbPath, _filename);
            if (!fileSystem.File.Exists(collectionDbPath))
            {
                return;
            }
            
            await _lock.WaitAsync(cancellationToken);
            try
            {
                await using var file = fileSystem.File.OpenRead(collectionDbPath);
                var newRoot = await JsonSerializer.DeserializeAsync<IEnumerable<T>>(file, JsonOptions, cancellationToken);
                if (newRoot is not null)
                {
                    foreach (var value in newRoot)
                    {
                        var key = _getId.GetValue(value) as string ?? throw new Exception($"cannot determine key of {value}");
                        _data[key] = value;
                    }
                }

            }
            catch (Exception e)
            {
                logger.LogError(e, "failed to read db json file at {Path}", collectionDbPath);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task TryWriteAsync(IFileSystem fileSystem, string dbPath, ILogger logger, CancellationToken cancellationToken = default)
        {
            if (!_dirty)
            {
                return;
            }
        
            var collectionDbPath = fileSystem.Path.Join(dbPath, _filename);
            logger.LogDebug("Dumping json db to disc {File}", collectionDbPath);
        
            var tmpFile = fileSystem.Path.GetTempFileName();
            var backupFile = fileSystem.Path.GetTempFileName();
            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (fileSystem.File.Exists(collectionDbPath))
                {
                    await using var file = fileSystem.File.OpenWrite(tmpFile);
                    await JsonSerializer.SerializeAsync(file, _data.Values, JsonOptions, cancellationToken);
                    fileSystem.File.Replace(tmpFile, collectionDbPath, backupFile);
                }
                else
                {
                    await using var file = fileSystem.File.OpenWrite(collectionDbPath);
                    await JsonSerializer.SerializeAsync(file, _data.Values, JsonOptions, cancellationToken);
                }
                _dirty = false;
            }
            catch (Exception e)
            {
                logger.LogError(e, "failed to save db {File}", collectionDbPath);
            }
            finally
            {
                _lock.Release();
                TryDeleteIfExists(tmpFile);
                TryDeleteIfExists(backupFile);
            }

            return;
            
            void TryDeleteIfExists(string path)
            {
                if (!fileSystem.File.Exists(path)) return;
                try
                {
                    fileSystem.File.Delete(path);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "failed to delete tmp db file {File}", path);
                }
            }
        }
        
        public int Count => _data.Count;

        public T? Get(string id) => _data.GetValueOrDefault(id);
        
        public async Task UpsertAsync(string id, T? value, CancellationToken cancellationToken = default)
        {
            if (value is null)
            {
                await WithWriteLockAsync(() => _data.TryRemove(id, out _), cancellationToken);
            }
            else
            {
                if (_data.TryGetValue(id, out var existing) && value.Equals(existing))
                {
                    return;
                }
                await WithWriteLockAsync(() =>
                {
                    _data[id] = value;
                    return true;
                }, cancellationToken);
            }
        }

        private async Task WithWriteLockAsync(Func<bool> block, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                _dirty = block();
            }
            finally
            {
                _lock.Release(1);
            }
        }

        public IEnumerator<T> GetEnumerator() => _data.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
