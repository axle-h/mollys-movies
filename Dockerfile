FROM mcr.microsoft.com/dotnet/sdk:5.0 AS dotnet-build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY MolliesMovies/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY MolliesMovies/ ./
RUN dotnet publish -c Release -o out

# TODO build ui

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=dotnet-build-env /app/out .
ENTRYPOINT ["dotnet", "MolliesMovies.dll"]