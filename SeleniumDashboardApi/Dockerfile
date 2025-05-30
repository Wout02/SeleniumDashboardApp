# Basis image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

# Restore en build
RUN dotnet restore SeleniumDashboardApi.csproj
RUN dotnet publish SeleniumDashboardApi.csproj -c Release -o /app/publish

# Publiceer image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SeleniumDashboardApi.dll"]
