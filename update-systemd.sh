#!/bin/bash
set -e

systemctl stop mollies-movies
rm -rf /usr/share/mollies-movies
rm -rf MolliesMovies/wwwroot

cd ui
yarn
yarn build --prod
cp -r dist/ui ../MolliesMovies/wwwroot

cd ..
dotnet publish -c Release -r linux-x64

mkdir -p /var/mollies-movies/movie-images
cp -r MolliesMovies/bin/Release/netcoreapp3.1/linux-x64/publish /usr/share/mollies-movies

systemctl start mollies-movies
