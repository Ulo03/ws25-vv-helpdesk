# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ServiceDesk/ServiceDesk.slnx ./
COPY ServiceDesk/ServiceDesk.Web/ServiceDesk.Web.csproj ServiceDesk/ServiceDesk.Web/

RUN dotnet restore ServiceDesk.slnx

COPY ServiceDesk/. ./ServiceDesk/

RUN dotnet publish ServiceDesk/ServiceDesk.Web/ServiceDesk.Web.csproj \
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

ENTRYPOINT ["dotnet", "ServiceDesk.Web.dll"]
