# Basis image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080

# Build image  
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files (let op de paden!)
COPY ["SeleniumDashboardApi/SeleniumDashboardApi.csproj", "SeleniumDashboardApi/"]
COPY ["SeleniumDashboard.Shared/SeleniumDashboard.Shared.csproj", "SeleniumDashboard.Shared/"]

# Restore dependencies
RUN dotnet restore "SeleniumDashboardApi/SeleniumDashboardApi.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/SeleniumDashboardApi"
RUN dotnet publish "SeleniumDashboardApi.csproj" -c Release -o /app/publish --no-restore

# Runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SeleniumDashboardApi.dll"]
