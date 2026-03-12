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

var prefix = 'riskmgmt-${environmentName}'
var acrName = replace('riskmgmt${environmentName}acr', '-', '')

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

module containerAppsEnv 'modules/containerAppsEnvironment.bicep' = {
  name: 'container-apps-env'
  params: {
    name: '${prefix}-env'
    location: location
  }
}

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

module app 'modules/containerApp.bicep' = {
  name: 'risk-management-app'
  params: {
    name: '${prefix}-app'
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
    envVars: [
      { name: 'DATABASE_URL', value: 'postgresql://${postgres.outputs.adminUsername}:${postgresAdminPassword}@${postgres.outputs.fqdn}:5432/risk_management?sslmode=require' }
      { name: 'OIDC_ISSUER', value: 'https://${keycloak.outputs.fqdn}/realms/risk-management' }
      { name: 'OIDC_CLIENT_ID', value: 'risk-management-app' }
      { name: 'OIDC_CLIENT_SECRET', value: oidcClientSecret }
      { name: 'OIDC_REDIRECT_URI', value: 'https://${prefix}-app.${location}.azurecontainerapps.io/auth/callback' }
      { name: 'OIDC_POST_LOGOUT_REDIRECT_URI', value: 'https://${prefix}-app.${location}.azurecontainerapps.io' }
      { name: 'OIDC_SCOPE', value: 'openid profile email' }
      { name: 'OIDC_ROLES_CLAIM_PATH', value: 'resource_access.risk-management-app.roles' }
      { name: 'PORT', value: '3000' }
    ]
  }
}

output acrLoginServer string = acr.outputs.loginServer
output keycloakFqdn string = keycloak.outputs.fqdn
output appFqdn string = app.outputs.fqdn
output postgresFqdn string = postgres.outputs.fqdn
