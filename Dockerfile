# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files (for layer caching)
COPY LMSDataExtraction.sln .
COPY LMSDataExtraction.Api/LMSDataExtraction.Api.csproj LMSDataExtraction.Api/
COPY LMSDataExtraction.Application/LMSDataExtraction.Application.csproj LMSDataExtraction.Application/
COPY LMSDataExtraction.Domain/LMSDataExtraction.Domain.csproj LMSDataExtraction.Domain/
COPY LMSDataExtraction.Infrastructure/LMSDataExtraction.Infrastructure.csproj LMSDataExtraction.Infrastructure/
COPY LMSDataExtraction.Tests/LMSDataExtraction.Tests.csproj LMSDataExtraction.Tests/

# Restore dependencies
RUN dotnet restore LMSDataExtraction.sln

# Copy the rest of the source code
COPY . .

# Build and publish the API project
RUN dotnet publish LMSDataExtraction.Api/LMSDataExtraction.Api.csproj \
    --configuration Release \
        --no-restore \
            --output /app/publish

            # Stage 2: Runtime
            FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
            WORKDIR /app

            # Copy published output from build stage
            COPY --from=build /app/publish .

            # Expose port 8080 (default for ASP.NET Core in containers)
            EXPOSE 8080

            ENV ASPNETCORE_ENVIRONMENT=Production
            ENV ASPNETCORE_URLS=http://+:8080

            ENTRYPOINT ["dotnet", "LMSDataExtraction.Api.dll"]
