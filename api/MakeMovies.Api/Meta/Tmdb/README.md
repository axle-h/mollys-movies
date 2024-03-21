# TMDB Client

[TMDB API](https://developer.themoviedb.org/reference/)

Client generated with [Kiota](https://learn.microsoft.com/en-us/openapi/kiota).

```bash
kiota generate -l CSharp -c TmdbClient -n MakeMovies.Api.Meta.Tmdb -d tmdb-api-v3.json -o . --include-path /3/find/* --include-path /3/configuration --exclude-backward-compatible --serializer Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory --deserializer Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory
```
