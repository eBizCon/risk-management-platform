@description('Name prefix for resources')
param prefix string

@description('Location for resources')
param location string

@description('Container image tag to deploy')
param imageTag string

@description('Container registry server (optional)')
param registryServer string = ''

@description('Container registry username (optional)')
param registryUsername string = ''

@secure()
@description('Container registry password/token (optional)')
param registryPassword string = ''

@description('Devin organization ID used to construct the sessions API endpoint')
param devinOrgId string

@description('Application Insights connection string for Function telemetry')
param appInsightsConnectionString string

@secure()
@description('Devin API key/token (optional depending on Devin API setup)')
param devinApiKey string = ''

@secure()
@description('Shared token expected from Azure Monitor webhook query string')
param alertWebhookToken string

var storageAccountName = take('devin${uniqueString(resourceGroup().id, prefix)}', 24)
var functionAppName = '${prefix}-devin-bridge'
var baseAppSettings = [
  {
    name: 'AzureWebJobsStorage'
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
  }
  {
    name: 'FUNCTIONS_WORKER_RUNTIME'
    value: 'dotnet-isolated'
  }
  {
    name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
    value: 'false'
  }
  {
    name: 'DOCKER_ENABLE_CI'
    value: 'true'
  }
  {
    name: 'DEVIN_ORG_ID'
    value: devinOrgId
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: appInsightsConnectionString
  }
  {
    name: 'DEVIN_API_KEY'
    value: devinApiKey
  }
  {
    name: 'ALERT_WEBHOOK_TOKEN'
    value: alertWebhookToken
  }
]

var registryAppSettings = !empty(registryPassword) ? [
  {
    name: 'DOCKER_REGISTRY_SERVER_URL'
    value: 'https://${registryServer}'
  }
  {
    name: 'DOCKER_REGISTRY_SERVER_USERNAME'
    value: registryUsername
  }
  {
    name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
    value: registryPassword
  }
] : []

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${prefix}-devin-bridge-plan'
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux,container'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|ghcr.io/ebizcon/risk-management-platform/devin-alert-bridge:${imageTag}'
      appSettings: concat(baseAppSettings, registryAppSettings)
      alwaysOn: false
    }
  }
}

output alertWebhookBaseUrl string = 'https://${functionApp.properties.defaultHostName}/api/alerts/devin'
output functionAppName string = functionApp.name
