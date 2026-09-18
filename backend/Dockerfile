# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy Directory.Build.props and csproj files for optimized layer caching
COPY Directory.Build.props ./
COPY src/Domain/Pqrsdf.Domain.csproj src/Domain/
COPY src/Application/Pqrsdf.Application.csproj src/Application/
COPY src/Infrastructure/Pqrsdf.Infrastructure.csproj src/Infrastructure/
COPY src/Api/Pqrsdf.Api.csproj src/Api/

# Restore dependencies
RUN dotnet restore src/Api/Pqrsdf.Api.csproj

# Copy remaining source code
COPY src/ src/

# Publish application in Release mode
RUN dotnet publish src/Api/Pqrsdf.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Configure default ports and environment
ENV ASPNETCORE_URLS=http://+:5023
EXPOSE 5023

# Copy published binaries
COPY --from=build /app/publish .

# Run as built-in non-root app user
USER app

ENTRYPOINT ["dotnet", "Pqrsdf.Api.dll"]
