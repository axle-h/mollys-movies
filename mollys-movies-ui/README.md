# Molly's Movies UI

Angular UI for mollys-movies services.

## Run

Requires Docker.

```bash
docker compose up
```

UI will be available on `http://localhost:5000`

## Development

Requires:

* Node 16
* Docker

```bash
# install npm packages
npm install

# start wiremock, which has mappings setup for the Molly's Movies API, available on localhost:8080
docker compose up wiremock

# start Angular dev server on localhost:4200
npm start
```

The wiremock mappings are built with [the seed CLI](stubs/cli.ts) & can be updated on a running Wiremock container:

```
npm run seed
```

The static mappings used by new Wiremock containers can be regenerated:

```
npm run seed -- -w
```

## API Client

The Molly's Movies API clients in `src/app/api/index.ts` and the models used by the seed in `stubs/api/model/index.ts` are generated via OpenAPI. To regenerate them:

```bash
npm run openapi-generator
```