# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and csproj files
COPY CryptoWeb.sln ./
COPY CryptoWeb.Shared/CryptoWeb.Shared.csproj CryptoWeb.Shared/
COPY CryptoWeb.Server/CryptoWeb.Server.csproj CryptoWeb.Server/

# Restore dependencies
RUN dotnet restore CryptoWeb.Server/CryptoWeb.Server.csproj

# Copy the rest of the source
COPY . .

# Publish Web API
WORKDIR /src/CryptoWeb.Server
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Koyeb requires the app to listen on 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
# Copy SQLite database
COPY CryptoWeb.Server/database.db /app/database.db
ENTRYPOINT ["dotnet", "CryptoWeb.Server.dll"]
