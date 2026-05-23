# ── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/AppTurismoIndustrial.Api.csproj .
RUN dotnet restore AppTurismoIndustrial.Api.csproj

COPY src/. .
RUN dotnet publish AppTurismoIndustrial.Api.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway injeta a variável PORT; fallback para 8080 em outros ambientes
ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080

EXPOSE 8080

# Usa shell form para que $PORT seja expandido em tempo de execução
CMD ASPNETCORE_URLS=http://+:${PORT} dotnet AppTurismoIndustrial.Api.dll
