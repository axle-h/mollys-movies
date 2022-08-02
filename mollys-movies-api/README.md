# Molly's Movies API

Monorepo of .NET mollys-movies services.

## Dependencies

* MongoDB
* RabbitMQ
* Plex Media Server
* Transmission

## Run

Requires Docker & Plex (as difficult to run in Docker).

1. Update your Plex URL & token in `docker-compose.yml`
2. Start app & dependencies: `docker compose up`
3. API will be available on `http://localhost:5000`

## Development

Requires:

* .NET 6
* Docker

1. Update your Plex URL & token in `docker-compose.yml`
2. Start dependent services: `docker compose up momgo rabbitmq`

Then you can e.g. run the API or the scraper:

```bash
dotnet run --project src/MollysMovies.Api --environment Development
dotnet run --project src/MollysMovies.Scraper --environment Development
```

Or the tests:

```bash
dotnet test ./src/MollysMovies.Tests # shared unit tests
dotnet test ./src/MollysMovies.Api.E2e # e2e tests for the API
dotnet test ./src/MollysMovies.Scraper.E2e # e2e tests for the scraper
```

## API Client

MollysMovies.Client is generated via OpenAPI. To regenerate it:

```bash
npx --package @openapitools/openapi-generator-cli openapi-generator-cli generate
```