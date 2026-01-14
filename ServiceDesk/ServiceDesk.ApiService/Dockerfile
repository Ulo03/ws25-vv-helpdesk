# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution & project files (for cache)
COPY ServiceDesk/ServiceDesk.slnx ./
COPY ServiceDesk/ServiceDesk.ApiService/ServiceDesk.ApiService.csproj ServiceDesk/ServiceDesk.ApiService/

RUN dotnet restore ServiceDesk.slnx

# Copy full source
COPY ServiceDesk/. ./ServiceDesk/

# Publish API
RUN dotnet publish ServiceDesk/ServiceDesk.ApiService/ServiceDesk.ApiService.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ServiceDesk.ApiService.dll"]
