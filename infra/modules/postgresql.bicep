@description('Name of the PostgreSQL server')
param name string

@description('Location for the PostgreSQL server')
param location string

@description('Administrator login name')
param adminUsername string = 'pgadmin'

@secure()
@description('Administrator password')
param adminPassword string

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' = {
  name: name
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '17'
    administratorLogin: adminUsername
    administratorLoginPassword: adminPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

resource firewallRuleAllowAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2022-12-01' = {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource riskManagementDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2022-12-01' = {
  parent: postgresServer
  name: 'risk-management'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource customerManagementDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2022-12-01' = {
  parent: postgresServer
  name: 'customer-management'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource customerManagementDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2022-12-01' = {
  parent: postgresServer
  name: 'customer_management'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource keycloakDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2022-12-01' = {
  parent: postgresServer
  name: 'keycloak'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

output fqdn string = postgresServer.properties.fullyQualifiedDomainName
output adminUsername string = adminUsername
