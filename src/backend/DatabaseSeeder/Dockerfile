# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY RiskManagementPlatform.slnx .
COPY DatabaseSeeder/DatabaseSeeder.csproj DatabaseSeeder/
COPY CustomerManagement.Infrastructure/CustomerManagement.Infrastructure.csproj CustomerManagement.Infrastructure/
COPY CustomerManagement.Domain/CustomerManagement.Domain.csproj CustomerManagement.Domain/
COPY CustomerManagement.Application/CustomerManagement.Application.csproj CustomerManagement.Application/
COPY RiskManagement.Infrastructure/RiskManagement.Infrastructure.csproj RiskManagement.Infrastructure/
COPY RiskManagement.Domain/RiskManagement.Domain.csproj RiskManagement.Domain/
COPY RiskManagement.Application/RiskManagement.Application.csproj RiskManagement.Application/
COPY ServiceDefaults/ServiceDefaults.csproj ServiceDefaults/
COPY SharedKernel/SharedKernel.csproj SharedKernel/

# Restore dependencies
RUN dotnet restore DatabaseSeeder/DatabaseSeeder.csproj

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish DatabaseSeeder/DatabaseSeeder.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DatabaseSeeder.dll"]
