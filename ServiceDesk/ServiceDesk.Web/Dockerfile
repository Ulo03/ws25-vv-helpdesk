# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy only what the API needs
COPY ServiceDesk/ServiceDesk.Web/ ServiceDesk/ServiceDesk.Web/
COPY ServiceDesk/ServiceDesk.Contracts/ ServiceDesk/ServiceDesk.Contracts/
COPY ServiceDesk/ServiceDesk.ServiceDefaults/ ServiceDesk/ServiceDesk.ServiceDefaults/

WORKDIR /src/ServiceDesk/ServiceDesk.Web

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ServiceDesk.Web.dll"]
