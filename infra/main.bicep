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
@description('OIDC client secret for the SvelteKit BFF')
param oidcClientSecret string = ''

@secure()
@description('Session encryption secret for the SvelteKit BFF (32+ chars)')
param sessionSecret string

@secure()
@description('Shared API key for service-to-service communication')
param serviceApiKey string

@secure()
@description('RabbitMQ password')
param rabbitmqPassword string

var prefix = 'riskmgmt-${environmentName}'
var acrName = replace('riskmgmt${environmentName}acr', '-', '')

// --- Container Registry ---
module acr 'modules/containerRegistry.bicep' = {
  name: 'container-registry'
  params: {
    name: acrName
    location: location
  }
}

// --- PostgreSQL ---
module postgres 'modules/postgresql.bicep' = {
  name: 'postgresql'
  params: {
    name: '${prefix}-pg'
    location: location
    adminPassword: postgresAdminPassword
  }
}

// --- Container Apps Environment ---
module containerAppsEnv 'modules/containerAppsEnvironment.bicep' = {
  name: 'container-apps-env'
  params: {
    name: '${prefix}-env'
    location: location
  }
}

// --- Keycloak ---
module keycloak 'modules/containerApp.bicep' = {
  name: 'keycloak'
  params: {
    name: '${prefix}-keycloak'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    image: 'quay.io/keycloak/keycloak:26.0'
    command: ['/opt/keycloak/bin/kc.sh', 'start']
    ingressPort: 8080
    ingressExternal: true
    minReplicas: 1
    maxReplicas: 1
    cpu: '0.5'
    memory: '1Gi'
    envVars: [
      { name: 'KC_DB', value: 'postgres' }
      { name: 'KC_DB_URL', value: 'jdbc:postgresql://${postgres.outputs.fqdn}:5432/keycloak?sslmode=require' }
      { name: 'KC_DB_USERNAME', value: postgres.outputs.adminUsername }
      { name: 'KC_DB_PASSWORD', value: postgresAdminPassword }
      { name: 'KC_HTTP_PORT', value: '8080' }
      { name: 'KC_PROXY_HEADERS', value: 'xforwarded' }
      { name: 'KC_HOSTNAME_STRICT', value: 'false' }
      { name: 'KC_HTTP_ENABLED', value: 'true' }
      { name: 'KEYCLOAK_ADMIN', value: 'admin' }
      { name: 'KEYCLOAK_ADMIN_PASSWORD', value: keycloakAdminPassword }
    ]
  }
}

// --- RabbitMQ ---
module rabbitmq 'modules/containerApp.bicep' = {
  name: 'rabbitmq'
  params: {
    name: '${prefix}-rabbitmq'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    image: 'rabbitmq:4-management'
    ingressPort: 5672
    ingressExternal: false
    ingressTransport: 'tcp'
    minReplicas: 1
    maxReplicas: 1
    cpu: '0.5'
    memory: '1Gi'
    envVars: [
      { name: 'RABBITMQ_DEFAULT_USER', value: 'risk' }
      { name: 'RABBITMQ_DEFAULT_PASS', value: rabbitmqPassword }
    ]
  }
}

// --- RiskManagement.Api ---
module riskApi 'modules/containerApp.bicep' = {
  name: 'risk-api'
  params: {
    name: '${prefix}-risk-api'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    image: '${acr.outputs.loginServer}/riskmanagement-api:latest'
    registryServer: acr.outputs.loginServer
    registryUsername: acr.outputs.adminUsername
    registryPassword: acr.outputs.adminPassword
    ingressPort: 8080
    ingressExternal: false
    minReplicas: 0
    maxReplicas: 5
    cpu: '0.5'
    memory: '1Gi'
    envVars: [
      { name: 'ConnectionStrings__DefaultConnection', value: 'Host=${postgres.outputs.fqdn};Port=5432;Database=risk_management;Username=${postgres.outputs.adminUsername};Password=${postgresAdminPassword};Ssl Mode=Require;Trust Server Certificate=true' }
      { name: 'ConnectionStrings__messaging', value: 'amqp://risk:${rabbitmqPassword}@${prefix}-rabbitmq.internal.${containerAppsEnv.outputs.defaultDomain}:5672' }
      { name: 'OIDC_ISSUER', value: 'https://${keycloak.outputs.fqdn}/realms/risk-management' }
      { name: 'CUSTOMER_SERVICE_URL', value: 'https://${prefix}-customer-api.internal.${containerAppsEnv.outputs.defaultDomain}' }
      { name: 'SERVICE_API_KEY', value: serviceApiKey }
      { name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED', value: 'true' }
    ]
  }
}

// --- CustomerManagement.Api ---
module customerApi 'modules/containerApp.bicep' = {
  name: 'customer-api'
  params: {
    name: '${prefix}-customer-api'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    image: '${acr.outputs.loginServer}/customermanagement-api:latest'
    registryServer: acr.outputs.loginServer
    registryUsername: acr.outputs.adminUsername
    registryPassword: acr.outputs.adminPassword
    ingressPort: 8080
    ingressExternal: false
    minReplicas: 0
    maxReplicas: 5
    cpu: '0.5'
    memory: '1Gi'
    envVars: [
      { name: 'ConnectionStrings__DefaultConnection', value: 'Host=${postgres.outputs.fqdn};Port=5432;Database=customer_management;Username=${postgres.outputs.adminUsername};Password=${postgresAdminPassword};Ssl Mode=Require;Trust Server Certificate=true' }
      { name: 'ConnectionStrings__messaging', value: 'amqp://risk:${rabbitmqPassword}@${prefix}-rabbitmq.internal.${containerAppsEnv.outputs.defaultDomain}:5672' }
      { name: 'OIDC_ISSUER', value: 'https://${keycloak.outputs.fqdn}/realms/risk-management' }
      { name: 'APPLICATION_SERVICE_URL', value: 'https://${prefix}-risk-api.internal.${containerAppsEnv.outputs.defaultDomain}' }
      { name: 'SERVICE_API_KEY', value: serviceApiKey }
      { name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED', value: 'true' }
    ]
  }
}

// --- Frontend (SvelteKit BFF) ---
module frontend 'modules/containerApp.bicep' = {
  name: 'frontend'
  params: {
    name: '${prefix}-frontend'
    location: location
    environmentId: containerAppsEnv.outputs.environmentId
    image: '${acr.outputs.loginServer}/risk-management-app:latest'
    registryServer: acr.outputs.loginServer
    registryUsername: acr.outputs.adminUsername
    registryPassword: acr.outputs.adminPassword
    ingressPort: 3000
    ingressExternal: true
    minReplicas: 0
    maxReplicas: 5
    cpu: '0.25'
    memory: '0.5Gi'
    envVars: [
      { name: 'RISK_MANAGEMENT_API_URL', value: 'https://${prefix}-risk-api.internal.${containerAppsEnv.outputs.defaultDomain}' }
      { name: 'CUSTOMER_SERVICE_URL', value: 'https://${prefix}-customer-api.internal.${containerAppsEnv.outputs.defaultDomain}' }
      { name: 'SERVICE_API_KEY', value: serviceApiKey }
      { name: 'SESSION_SECRET', value: sessionSecret }
      { name: 'OIDC_ISSUER', value: 'https://${keycloak.outputs.fqdn}/realms/risk-management' }
      { name: 'OIDC_CLIENT_ID', value: 'risk-management-app' }
      { name: 'OIDC_CLIENT_SECRET', value: oidcClientSecret }
      { name: 'OIDC_REDIRECT_URI', value: 'https://${prefix}-frontend.${containerAppsEnv.outputs.defaultDomain}/auth/callback' }
      { name: 'OIDC_POST_LOGOUT_REDIRECT_URI', value: 'https://${prefix}-frontend.${containerAppsEnv.outputs.defaultDomain}' }
      { name: 'OIDC_SCOPE', value: 'openid profile email' }
      { name: 'OIDC_ROLES_CLAIM_PATH', value: 'resource_access.risk-management-app.roles' }
      { name: 'PORT', value: '3000' }
      { name: 'NODE_ENV', value: 'production' }
    ]
  }
}

output acrLoginServer string = acr.outputs.loginServer
output keycloakFqdn string = keycloak.outputs.fqdn
output frontendFqdn string = frontend.outputs.fqdn
output riskApiFqdn string = riskApi.outputs.fqdn
output customerApiFqdn string = customerApi.outputs.fqdn
output postgresFqdn string = postgres.outputs.fqdn
