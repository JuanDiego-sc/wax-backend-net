# syntax=docker/dockerfile:1.7

ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY wax-backend-net.sln ./
COPY API/API.csproj                       API/
COPY Application/Application.csproj       Application/
COPY Domain/Domain.csproj                 Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Persistence/Persistence.csproj       Persistence/
COPY UnitTests/UnitTests.csproj           UnitTests/

RUN dotnet restore wax-backend-net.sln

COPY . .

RUN dotnet publish API/API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/* \
 && groupadd --system --gid 1001 app \
 && useradd --system --uid 1001 --gid app --no-create-home app

COPY --from=build --chown=app:app /app/publish ./

USER app
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD curl --fail --silent http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "API.dll"]
