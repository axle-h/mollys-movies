FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS dotnet-build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY MolliesMovies/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY MolliesMovies/ ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=dotnet-build-env /app/out .
ENTRYPOINT ["dotnet", "MolliesMovies.dll"]