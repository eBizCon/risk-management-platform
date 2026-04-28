@description('Location for all resources')
param location string = resourceGroup().location

@description('Environment name (e.g. dev, staging, prod)')
param environmentName string

@secure()
@description('PostgreSQL administrator password')
param postgresAdminPassword string

@secure()
@description('Keycloak administrator password')
param keycloakAdminPassword string

@secure()
@description('OIDC client secret for the SvelteKit app')
param oidcClientSecret string = ''

@secure()
@description('Service API Key for internal service authentication')
param serviceApiKey string

@secure()
@description('RabbitMQ administrator password')
param rabbitmqPassword string

@secure()
@description('GitHub Container Registry token (optional, for private images)')
param ghcrToken string = ''

@description('Container image tag to deploy (e.g. latest, branch-name)')
param imageTag string = 'latest'

var prefix = 'riskmgmt-${environmentName}'
var acrName = replace('riskmgmt${environmentName}acr', '-', '')

// Compute the deployed app FQDN for Keycloak redirect URI configuration
var appFqdn = '${prefix}-app.${containerAppsEnv.outputs.defaultDomain}'

// Inject deployed app URL into realm JSON so Keycloak accepts redirects from the deployed frontend
var rawRealmJson = loadTextContent('../dev/keycloak/import/risk-management-realm.json')
var deployedRealmJson = replace(
  replace(
    replace(rawRealmJson,
      '"http://localhost:5173/*", "http://localhost:5227/*"',
      '"http://localhost:5173/*", "http://localhost:5227/*", "https://${appFqdn}/*"'
    ),
    '"http://localhost:5173", "http://localhost:5227"',
    '"http://localhost:5173", "http://localhost:5227", "https://${appFqdn}"'
  ),
  '"http://localhost:5173/*##http://localhost:5227/*"',
  '"http://localhost:5173/*##http://localhost:5227/*##https://${appFqdn}/*"'
)

// Fallback to dev-client-secret from realm JSON when OIDC_CLIENT_SECRET is not provided
var effectiveOidcClientSecret = !empty(oidcClientSecret) ? oidcClientSecret : 'dev-client-secret'

// Deterministic session secret derived from resource group (stable across redeployments)
var generatedSessionSecret = '${uniqueString(resourceGroup().id, 'session-secret')}-${uniqueString(resourceGroup().id, prefix)}-secret-key!!'

// ACR is optional - can use GHCR instead
module acr 'modules/containerRegistry.bicep' = {
  name: 'container-registry'
  params: {
    name: acrName
    location: location
  }
}

module postgres 'modules/postgresql.bicep' = {
  name: 'postgresql'
  params: {
    name: '${prefix}-pg'
    location: location
    adminPassword: postgresAdminPassword
  }
}

module appInsights 'modules/applicationInsights.bicep' = {
  name: 'application-insights'
  params: {
    prefix: prefix
    location: location
  }
}

module containerAppsEnv 'modules/containerAppsEnvironment.bicep' = {
  name: 'container-apps-env'
  params: {
    name: '${prefix}-env'
    location: location
  }
}

module keycloak 'modules/keycloak.bicep' = {
  name: 'keycloak'
  params: {
    name: '${prefix}-keycloak'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    postgresFqdn: postgres.outputs.fqdn
    postgresUsername: postgres.outputs.adminUsername
    postgresPassword: postgresAdminPassword
    keycloakAdminPassword: keycloakAdminPassword
    realmImportJson: deployedRealmJson
  }
}

module databaseSeeder 'modules/databaseSeeder.bicep' = {
  name: 'database-seeder'
  params: {
    name: '${prefix}-seeder'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    image: 'ghcr.io/ebizcon/risk-management-platform/databaseseeder:${imageTag}'
    registryServer: !empty(ghcrToken) ? 'ghcr.io' : ''
    registryUsername: !empty(ghcrToken) ? 'ebizcon' : ''
    registryPassword: ghcrToken
    customerConnectionString: 'Host=${postgres.outputs.fqdn};Database=customer-management;Username=${postgres.outputs.adminUsername};Password=${postgresAdminPassword};SSL Mode=Require'
    riskConnectionString: 'Host=${postgres.outputs.fqdn};Database=risk-management;Username=${postgres.outputs.adminUsername};Password=${postgresAdminPassword};SSL Mode=Require'
  }
  dependsOn: [
    rabbitmq
  ]
}

module app 'modules/containerApp.bicep' = {
  name: 'risk-management-app'
  params: {
    name: '${prefix}-app'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    image: 'ghcr.io/ebizcon/risk-management-platform/risk-management-app:${imageTag}'
    registryServer: !empty(ghcrToken) ? 'ghcr.io' : ''
    registryUsername: !empty(ghcrToken) ? 'ebizcon' : ''
    registryPassword: ghcrToken
    ingressPort: 3000
    ingressExternal: true
    minReplicas: 0
    maxReplicas: 5
    envVars: [
      { name: 'DATABASE_URL', value: 'postgresql://${postgres.outputs.adminUsername}:${postgresAdminPassword}@${postgres.outputs.fqdn}:5432/risk_management?sslmode=require' }
      { name: 'SESSION_SECRET', value: generatedSessionSecret }
      { name: 'OIDC_ISSUER', value: 'https://${keycloak.outputs.fqdn}/realms/risk-management' }
      { name: 'OIDC_CLIENT_ID', value: 'risk-management-platform' }
      { name: 'OIDC_CLIENT_SECRET', value: effectiveOidcClientSecret }
      { name: 'OIDC_REDIRECT_URI', value: 'https://${appFqdn}/auth/callback' }
      { name: 'OIDC_POST_LOGOUT_REDIRECT_URI', value: 'https://${appFqdn}' }
      { name: 'OIDC_SCOPE', value: 'openid profile email' }
      { name: 'OIDC_ROLES_CLAIM_PATH', value: 'realm_access.roles' }
      { name: 'RISK_MANAGEMENT_API_URL', value: 'https://${prefix}-risk-api.${containerAppsEnv.outputs.defaultDomain}' }
      { name: 'CUSTOMER_SERVICE_URL', value: 'https://${prefix}-customer-api.${containerAppsEnv.outputs.defaultDomain}' }
      { name: 'SERVICE_API_KEY', value: serviceApiKey }
      { name: 'PORT', value: '3000' }
    ]
  }
}

module rabbitmq 'modules/rabbitmq.bicep' = {
  name: 'rabbitmq'
  params: {
    name: '${prefix}-rabbitmq'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    adminPassword: rabbitmqPassword
  }
}

module customerApi 'modules/containerApp.bicep' = {
  name: 'customer-api'
  params: {
    name: '${prefix}-customer-api'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    image: 'ghcr.io/ebizcon/risk-management-platform/customermanagement-api:${imageTag}'
    registryServer: !empty(ghcrToken) ? 'ghcr.io' : ''
    registryUsername: !empty(ghcrToken) ? 'ebizcon' : ''
    registryPassword: ghcrToken
    ingressPort: 8080
    ingressExternal: true
    minReplicas: 0
    maxReplicas: 5
    envVars: [
      { name: 'ConnectionStrings__DefaultConnection', value: 'Host=${postgres.outputs.fqdn};Database=customer-management;Username=${postgres.outputs.adminUsername};Password=${postgresAdminPassword};SSL Mode=Require' }
      { name: 'ConnectionStrings__messaging', value: 'amqp://risk:${rabbitmqPassword}@${rabbitmq.outputs.fqdn}:5672' }
      { name: 'RabbitMQ__ConnectionString', value: 'amqp://risk:${rabbitmqPassword}@${rabbitmq.outputs.fqdn}:5672' }
      { name: 'APPLICATION_SERVICE_URL', value: 'https://${prefix}-risk-api.${containerAppsEnv.outputs.defaultDomain}' }
      { name: 'SERVICE_API_KEY', value: serviceApiKey }
      { name: 'OIDC_ISSUER', value: 'https://${keycloak.outputs.fqdn}/realms/risk-management' }
      { name: 'OIDC_ROLES_CLAIM_PATH', value: 'realm_access.roles' }
      { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.outputs.connectionString }
    ]
  }
}

module riskApi 'modules/containerApp.bicep' = {
  name: 'risk-api'
  params: {
    name: '${prefix}-risk-api'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    image: 'ghcr.io/ebizcon/risk-management-platform/riskmanagement-api:${imageTag}'
    registryServer: !empty(ghcrToken) ? 'ghcr.io' : ''
    registryUsername: !empty(ghcrToken) ? 'ebizcon' : ''
    registryPassword: ghcrToken
    ingressPort: 8080
    ingressExternal: true
    minReplicas: 0
    maxReplicas: 5
    envVars: [
      { name: 'ConnectionStrings__DefaultConnection', value: 'Host=${postgres.outputs.fqdn};Database=risk-management;Username=${postgres.outputs.adminUsername};Password=${postgresAdminPassword};SSL Mode=Require' }
      { name: 'ConnectionStrings__messaging', value: 'amqp://risk:${rabbitmqPassword}@${rabbitmq.outputs.fqdn}:5672' }
      { name: 'RabbitMQ__ConnectionString', value: 'amqp://risk:${rabbitmqPassword}@${rabbitmq.outputs.fqdn}:5672' }
      { name: 'CUSTOMER_SERVICE_URL', value: 'https://${prefix}-customer-api.${containerAppsEnv.outputs.defaultDomain}' }
      { name: 'SERVICE_API_KEY', value: serviceApiKey }
      { name: 'OIDC_ISSUER', value: 'https://${keycloak.outputs.fqdn}/realms/risk-management' }
      { name: 'OIDC_ROLES_CLAIM_PATH', value: 'realm_access.roles' }
      { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.outputs.connectionString }
    ]
  }
}

output acrLoginServer string = acr.outputs.loginServer
output keycloakFqdn string = keycloak.outputs.fqdn
output keycloakAdminUsername string = keycloak.outputs.adminUsername
output appFqdn string = app.outputs.fqdn
output postgresFqdn string = postgres.outputs.fqdn
output rabbitMqFqdn string = rabbitmq.outputs.fqdn
output customerApiFqdn string = customerApi.outputs.fqdn
output riskApiFqdn string = riskApi.outputs.fqdn
output appInsightsConnectionString string = appInsights.outputs.connectionString
output databaseSeederJobName string = databaseSeeder.outputs.jobName
