FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DevinAlertBridge.Function/DevinAlertBridge.Function.csproj DevinAlertBridge.Function/
RUN dotnet restore DevinAlertBridge.Function/DevinAlertBridge.Function.csproj

COPY DevinAlertBridge.Function/ DevinAlertBridge.Function/
RUN dotnet publish DevinAlertBridge.Function/DevinAlertBridge.Function.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
WORKDIR /home/site/wwwroot
COPY --from=build /app/publish .
