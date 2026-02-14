FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/src/LifeSwap.Api/LifeSwap.Api.csproj backend/src/LifeSwap.Api/
RUN dotnet restore backend/src/LifeSwap.Api/LifeSwap.Api.csproj

COPY backend/src/LifeSwap.Api/ backend/src/LifeSwap.Api/
RUN dotnet publish backend/src/LifeSwap.Api/LifeSwap.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends gosu \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY backend/src/LifeSwap.Api/entrypoint.sh /entrypoint.sh

RUN mkdir -p /data \
    && chown -R app:app /app \
    && chmod +x /entrypoint.sh

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["/entrypoint.sh"]