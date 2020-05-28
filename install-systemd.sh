#!/bin/bash
set -e

cd ui
yarn
yarn build --prod
cp -r dist/ui ../MolliesMovies/wwwroot

cd ..
dotnet publish -c Release -r linux-x64

mkdir -p /var/mollies-movies/movie-images
cp -r MolliesMovies/bin/Release/netcoreapp3.1/linux-x64/publish /usr/share/mollies-movies
cp scripts/mollies-movies.service /etc/systemd/system/
systemctl daemon-reload

cp -f scripts/torrent-done /usr/bin/torrent-done
chmod +x /usr/bin/torrent-done