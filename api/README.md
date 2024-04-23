# Make Movies API

.NET API for movie management via [Jellyfin](https://jellyfin.org) & [Transmission](https://transmissionbt.com/).

Can scrape movies from YTS and meta from [TMDB](https://www.themoviedb.org) & [OMDB](https://www.omdbapi.com).

This is a super simple, not scalable, toy project.

## Test

There are some basic integration tests.

```bash
dotnet test
```

## Run

Requires [.NET 8+](https://dotnet.microsoft.com/en-us/download)

```bash
# Optional socks proxy for connections to torrent sites (recommended)
export Scrape__ProxyUrl=socks5://localhost:1080

# Path to a writeable folder for caching movie image files
export Meta__ImagePath=/var/make-movies/images

# Your access token for the TMDB API
export Meta__Tmdb__AccessToken=my-tmdb-access-token

# Your access token for the OMDB API
export Meta__Omdb__ApiKey=my-omdb-access-token

# URL & API key for your Jellyfin server
export Library__Jellyfin__Url=http://localhost:8096
export Library__Jellyfin__ApiKey=my-jellyfin-api-key

# Path to your Jellyfin movie library
export Library__MovieLibraryPath=/mnt/storage/movies

# Path to your Transmission download folder
export Library__DownloadsPath=/mnt/storage/downloads

# URL for your Transmission server
export Download__Transmission__Url=http://localhost:9091/transmission/

# Path to a writeable folder for saving app metadata
export Db__Path=/var/make-movies

dotnet run --project MakeMovies.Api
```

## Deploy

For simplicity, I run this on Ubuntu via systemd.

1. Build and copy an export to a central place.
    ```shell
    mkdir -p /opt/make-movies/api
    dotnet publish MakeMovies.Api/MakeMovies.Api.csproj  -c Release -o /opt/make-movies/api
    ```
2. Create systemd unit in `/etc/systemd/system/make-movies-api.service`:

    ```ini
    [Unit]
    Description=Make movies API
    Wants=network-online.target
    After=network-online.target

    [Service]
    User=alex
    Group=alex
    Type=notify
    Restart=on-failure
    EnvironmentFile=/var/make-movies/.env
    WorkingDirectory=/opt/make-movies/api
    ExecStart=/opt/make-movies/api/MakeMovies.Api --urls "http://0.0.0.0:5000" --environment Production

    [Install]
    WantedBy=default.target
    ```
    > Note: set a user and group that has access to your movie library & transmission download folder.
3. Create data directories: `mkdir -p /var/make-movies/images`
4. Configure via environment variables in `/var/make-movies/.env` e.g.
    ```ini
    Scrape__ProxyUrl=socks5://localhost:1080
    Meta__ImagePath=/var/make-movies/images
    Meta__Tmdb__AccessToken=my-tmdb-access-token
    Meta__Omdb__ApiKey=my-omdb-access-token
    Library__Jellyfin__Url=http://localhost:8096
    Library__Jellyfin__ApiKey=my-jellyfin-api-key
    Library__MovieLibraryPath=/mnt/storage/movies
    Library__DownloadsPath=/mnt/storage/downloads
    Download__Transmission__Url=http://localhost:9091/transmission/
    Db__Path=/var/make-movies
    ```
5. Reload systemd, start and enable the service:
    ```shell
    sudo systemctl daemon-reload
    sudo systemctl enable make-movies-api
    sudo systemctl start make-movies-api
    ```
6. Check health with `curl http://localhost:5000/health/ready`