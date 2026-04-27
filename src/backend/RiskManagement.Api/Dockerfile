# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY RiskManagementPlatform.slnx .
COPY RiskManagement.Api/RiskManagement.Api.csproj RiskManagement.Api/
COPY RiskManagement.Application/RiskManagement.Application.csproj RiskManagement.Application/
COPY RiskManagement.Domain/RiskManagement.Domain.csproj RiskManagement.Domain/
COPY RiskManagement.Infrastructure/RiskManagement.Infrastructure.csproj RiskManagement.Infrastructure/
COPY ServiceDefaults/ServiceDefaults.csproj ServiceDefaults/
COPY SharedKernel/SharedKernel.csproj SharedKernel/

# Restore dependencies
RUN dotnet restore RiskManagement.Api/RiskManagement.Api.csproj

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish RiskManagement.Api/RiskManagement.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser

COPY --from=build /app/publish .

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RiskManagement.Api.dll"]
