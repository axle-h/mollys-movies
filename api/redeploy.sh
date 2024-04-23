#!/bin/bash
set -e

rm -rf /opt/make-movies/api
mkdir -p /opt/make-movies/api

dotnet publish MakeMovies.Api/MakeMovies.Api.csproj  -c Release -o /opt/make-movies/api
chown -R alex:alex /opt/make-movies/api

systemctl restart make-movies-api.service
