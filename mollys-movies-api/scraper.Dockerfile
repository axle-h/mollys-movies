FROM mcr.microsoft.com/dotnet/sdk:6.0 AS dotnet-build-env
WORKDIR /app

COPY src/MollysMovies.Scraper src/MollysMovies.Scraper
COPY src/MollysMovies.Common src/MollysMovies.Common
COPY src/MollysMovies.ScraperClient src/MollysMovies.ScraperClient
RUN dotnet publish -c Release -o dist src/MollysMovies.Scraper

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=dotnet-build-env /app/dist .

ENV SCRAPER__IMAGEPATH /movie-images
ENV SCRAPER__MOVIELIBRARYPATH /movie-library
ENV SCRAPER__DOWNLOADSPATH /downloads

HEALTHCHECK CMD curl --fail http://localhost/health/live || exit
CMD dotnet MollysMovies.Scraper.dll