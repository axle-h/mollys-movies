FROM mcr.microsoft.com/dotnet/sdk:6.0 AS dotnet-build-env
WORKDIR /app

COPY src/MollysMovies.Callback src/MollysMovies.Callback
COPY src/MollysMovies.ScraperClient src/MollysMovies.ScraperClient
RUN dotnet publish -c Release -o dist src/MollysMovies.Callback

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine3.15

COPY --from=dotnet-build-env /app/dist /opt/mollys-movies
RUN mkdir /config && echo $'{\n\
    "alt-speed-down": 50,\n\
    "alt-speed-enabled": false,\n\
    "alt-speed-time-begin": 540,\n\
    "alt-speed-time-day": 127,\n\
    "alt-speed-time-enabled": false,\n\
    "alt-speed-time-end": 1020,\n\
    "alt-speed-up": 50,\n\
    "bind-address-ipv4": "0.0.0.0",\n\
    "bind-address-ipv6": "::",\n\
    "blocklist-enabled": false,\n\
    "blocklist-url": "http://www.example.com/blocklist",\n\
    "cache-size-mb": 4,\n\
    "dht-enabled": true,\n\
    "download-dir": "/downloads",\n\
    "download-queue-enabled": true,\n\
    "download-queue-size": 5,\n\
    "encryption": 1,\n\
    "idle-seeding-limit": 30,\n\
    "idle-seeding-limit-enabled": false,\n\
    "incomplete-dir": "/downloads/incomplete",\n\
    "incomplete-dir-enabled": true,\n\
    "lpd-enabled": false,\n\
    "message-level": 2,\n\
    "peer-congestion-algorithm": "",\n\
    "peer-id-ttl-hours": 6,\n\
    "peer-limit-global": 200,\n\
    "peer-limit-per-torrent": 50,\n\
    "peer-port": 51413,\n\
    "peer-port-random-high": 65535,\n\
    "peer-port-random-low": 49152,\n\
    "peer-port-random-on-start": false,\n\
    "peer-socket-tos": "default",\n\
    "pex-enabled": true,\n\
    "port-forwarding-enabled": false,\n\
    "preallocation": 1,\n\
    "prefetch-enabled": 1,\n\
    "queue-stalled-enabled": true,\n\
    "queue-stalled-minutes": 30,\n\
    "ratio-limit": 2,\n\
    "ratio-limit-enabled": false,\n\
    "rename-partial-files": true,\n\
    "rpc-authentication-required": false,\n\
    "rpc-bind-address": "0.0.0.0",\n\
    "rpc-enabled": true,\n\
    "rpc-password": "{1ddd3f1f6a71d655cde7767242a23a575b44c909n5YuRT.f",\n\
    "rpc-port": 9091,\n\
    "rpc-url": "/transmission/",\n\
    "rpc-username": "",\n\
    "rpc-host-whitelist": "127.0.0.1",\n\
    "rpc-host-whitelist-enabled": false,\n\
    "rpc-whitelist": "127.0.0.1",\n\
    "rpc-whitelist-enabled": false,\n\
    "scrape-paused-torrents-enabled": true,\n\
    "script-torrent-done-enabled": true,\n\
    "script-torrent-done-filename": "/config/callback.sh",\n\
    "seed-queue-enabled": false,\n\
    "seed-queue-size": 10,\n\
    "speed-limit-down": 100,\n\
    "speed-limit-down-enabled": false,\n\
    "speed-limit-up": 100,\n\
    "speed-limit-up-enabled": false,\n\
    "start-added-torrents": true,\n\
    "trash-original-torrent-files": false,\n\
    "umask": 2,\n\
    "upload-slots-per-torrent": 14,\n\
    "utp-enabled": true,\n\
    "watch-dir": "/watch",\n\
    "watch-dir-enabled": false\n\
}' > /config/settings.json && echo $'#/bin/sh\n\
dotnet /opt/mollys-movies/MollysMovies.Callback.dll --TorrentId $TR_TORRENT_ID' > /config/callback.sh \
&& chmod +x /config/callback.sh
RUN apk add --no-cache transmission-daemon
EXPOSE 9091 51413/tcp 51413/udp
VOLUME /downloads
CMD transmission-daemon --foreground --config-dir /config