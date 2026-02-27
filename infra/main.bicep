targetScope = 'resourceGroup'

@description('Primary location for all resources')
param location string = resourceGroup().location

@description('Name of the project (used as prefix for resource names)')
param projectName string = 'riskmanagement'

@description('Unique suffix for resource names')
param resourceSuffix string = uniqueString(resourceGroup().id)

@description('Container image for the risk-management-platform app')
param appImageName string = ''

@description('Keycloak admin username')
@secure()
param keycloakAdminUsername string = 'admin'

@description('Keycloak admin password')
@secure()
param keycloakAdminPassword string

@description('OIDC Client ID for the application')
param oidcClientId string = 'risk-management-platform'

@description('OIDC Scope')
param oidcScope string = 'openid profile email'

@description('OIDC Roles Claim Path')
param oidcRolesClaimPath string = 'realm_access.roles'

// ============================================================================
// Variables
// ============================================================================

var abbrs = {
  containerRegistry: 'cr'
  containerAppsEnvironment: 'cae'
  containerApp: 'ca'
  logAnalyticsWorkspace: 'log'
  managedIdentity: 'id'
  storageAccount: 'st'
  fileShare: 'share'
}

var resourceName = '${projectName}${resourceSuffix}'
var containerRegistryName = '${abbrs.containerRegistry}${resourceName}'
var environmentName = '${abbrs.containerAppsEnvironment}-${projectName}-${resourceSuffix}'
var logAnalyticsName = '${abbrs.logAnalyticsWorkspace}-${projectName}-${resourceSuffix}'
var managedIdentityName = '${abbrs.managedIdentity}-${projectName}-${resourceSuffix}'
var storageAccountName = take('${abbrs.storageAccount}${replace(resourceName, '-', '')}', 24)
var appName = '${abbrs.containerApp}-${projectName}-app-${resourceSuffix}'
var keycloakAppName = '${abbrs.containerApp}-${projectName}-kc-${resourceSuffix}'

// ============================================================================
// Log Analytics Workspace
// ============================================================================

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// ============================================================================
// User-Assigned Managed Identity
// ============================================================================

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
}

// ============================================================================
// Container Registry
// ============================================================================

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    anonymousPullEnabled: false
  }
}

// AcrPull role assignment for the managed identity
resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, managedIdentity.id, '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ============================================================================
// Storage Account (for SQLite persistence via Azure Files)
// ============================================================================

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource appFileShare 'Microsoft.Storage/storageAccounts/fileServices/fileShares@2023-05-01' = {
  parent: fileService
  name: 'appdata'
  properties: {
    shareQuota: 1
  }
}

resource keycloakFileShare 'Microsoft.Storage/storageAccounts/fileServices/fileShares@2023-05-01' = {
  parent: fileService
  name: 'keycloakdata'
  properties: {
    shareQuota: 1
  }
}

// ============================================================================
// Container Apps Environment
// ============================================================================

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: environmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// Storage mounts for the Container Apps Environment
resource appStorage 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  parent: containerAppsEnvironment
  name: 'appdata'
  properties: {
    azureFile: {
      accountName: storageAccount.name
      accountKey: storageAccount.listKeys().keys[0].value
      shareName: appFileShare.name
      accessMode: 'ReadWrite'
    }
  }
}

resource keycloakStorage 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  parent: containerAppsEnvironment
  name: 'keycloakdata'
  properties: {
    azureFile: {
      accountName: storageAccount.name
      accountKey: storageAccount.listKeys().keys[0].value
      shareName: keycloakFileShare.name
      accessMode: 'ReadWrite'
    }
  }
}

// ============================================================================
// Keycloak Container App
// ============================================================================

resource keycloakApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: keycloakAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        corsPolicy: {
          allowedOrigins: ['*']
          allowedMethods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS']
          allowedHeaders: ['*']
        }
      }
      secrets: [
        {
          name: 'keycloak-admin-username'
          value: keycloakAdminUsername
        }
        {
          name: 'keycloak-admin-password'
          value: keycloakAdminPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'keycloak'
          // Use the official Keycloak image
          image: 'quay.io/keycloak/keycloak:26.0'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'KC_BOOTSTRAP_ADMIN_USERNAME'
              secretRef: 'keycloak-admin-username'
            }
            {
              name: 'KC_BOOTSTRAP_ADMIN_PASSWORD'
              secretRef: 'keycloak-admin-password'
            }
            {
              name: 'KC_HOSTNAME_STRICT'
              value: 'false'
            }
            {
              name: 'KC_HOSTNAME_STRICT_HTTPS'
              value: 'false'
            }
            {
              name: 'KC_PROXY_HEADERS'
              value: 'xforwarded'
            }
            {
              name: 'KC_HTTP_ENABLED'
              value: 'true'
            }
          ]
          volumeMounts: [
            {
              volumeName: 'keycloakdata'
              mountPath: '/opt/keycloak/data'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
      volumes: [
        {
          name: 'keycloakdata'
          storageName: 'keycloakdata'
          storageType: 'AzureFile'
        }
      ]
    }
  }
  dependsOn: [
    keycloakStorage
  ]
}

// ============================================================================
// Risk Management Platform Container App
// ============================================================================

resource riskApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: appName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: managedIdentity.id
        }
      ]
      ingress: {
        external: true
        targetPort: 3000
        transport: 'http'
        corsPolicy: {
          allowedOrigins: ['*']
          allowedMethods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS']
          allowedHeaders: ['*']
        }
      }
      secrets: [
        {
          name: 'oidc-client-secret'
          value: ''
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'risk-management-app'
          // Initial deployment uses placeholder image; CI/CD updates this
          image: !empty(appImageName) ? appImageName : 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'NODE_ENV'
              value: 'production'
            }
            {
              name: 'PORT'
              value: '3000'
            }
            {
              name: 'HOST'
              value: '0.0.0.0'
            }
            {
              name: 'DATABASE_PATH'
              value: '/data/data.db'
            }
            {
              name: 'ORIGIN'
              value: 'https://${appName}.${containerAppsEnvironment.properties.defaultDomain}'
            }
            {
              name: 'OIDC_ISSUER'
              value: 'https://${keycloakAppName}.${containerAppsEnvironment.properties.defaultDomain}/realms/risk-management'
            }
            {
              name: 'OIDC_CLIENT_ID'
              value: oidcClientId
            }
            {
              name: 'OIDC_CLIENT_SECRET'
              secretRef: 'oidc-client-secret'
            }
            {
              name: 'OIDC_REDIRECT_URI'
              value: 'https://${appName}.${containerAppsEnvironment.properties.defaultDomain}/auth/callback'
            }
            {
              name: 'OIDC_POST_LOGOUT_REDIRECT_URI'
              value: 'https://${appName}.${containerAppsEnvironment.properties.defaultDomain}/'
            }
            {
              name: 'OIDC_SCOPE'
              value: oidcScope
            }
            {
              name: 'OIDC_ROLES_CLAIM_PATH'
              value: oidcRolesClaimPath
            }
          ]
          volumeMounts: [
            {
              volumeName: 'appdata'
              mountPath: '/data'
            }
          ]
        }
      ]
      // SQLite on Azure Files does not support concurrent access from multiple replicas.
      // Keep maxReplicas at 1 until migrating to a managed database (e.g. PostgreSQL).
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
      volumes: [
        {
          name: 'appdata'
          storageName: 'appdata'
          storageType: 'AzureFile'
        }
      ]
    }
  }
  dependsOn: [
    acrPullRoleAssignment
    appStorage
    keycloakApp
  ]
}

// ============================================================================
// Outputs
// ============================================================================

output containerRegistryLoginServer string = containerRegistry.properties.loginServer
output containerRegistryName string = containerRegistry.name
output appFqdn string = riskApp.properties.configuration.ingress.fqdn
output appUrl string = 'https://${riskApp.properties.configuration.ingress.fqdn}'
output keycloakFqdn string = keycloakApp.properties.configuration.ingress.fqdn
output keycloakUrl string = 'https://${keycloakApp.properties.configuration.ingress.fqdn}'
output managedIdentityClientId string = managedIdentity.properties.clientId
output managedIdentityName string = managedIdentity.name
output resourceGroupName string = resourceGroup().name
output appName string = riskApp.name
output keycloakAppName string = keycloakApp.name
