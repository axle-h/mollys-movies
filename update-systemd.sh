#!/bin/bash
set -e

rm -rf MolliesMovies/wwwroot

cd ui
npm install
npm run build -- --prod
cp -r dist/ui ../MolliesMovies/wwwroot

cd ..
dotnet publish -c Release -r linux-x64

systemctl stop mollies-movies
rm -rf /usr/share/mollies-movies
mkdir -p /var/mollies-movies/movie-images
cp -r MolliesMovies/bin/Release/net5.0/linux-x64/publish /usr/share/mollies-movies

systemctl start mollies-movies
