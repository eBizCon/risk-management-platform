using './main.bicep'

param location = readEnvironmentVariable('AZURE_LOCATION', 'westeurope')
param projectName = readEnvironmentVariable('AZURE_PROJECT_NAME', 'riskmanagement')
param keycloakAdminPassword = readEnvironmentVariable('KEYCLOAK_ADMIN_PASSWORD', '')
param keycloakAdminUsername = readEnvironmentVariable('KEYCLOAK_ADMIN_USERNAME', 'admin')
param oidcClientId = readEnvironmentVariable('OIDC_CLIENT_ID', 'risk-management-platform')
param oidcScope = readEnvironmentVariable('OIDC_SCOPE', 'openid profile email')
param oidcRolesClaimPath = readEnvironmentVariable('OIDC_ROLES_CLAIM_PATH', 'realm_access.roles')
