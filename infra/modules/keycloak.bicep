@description('Name of the Keycloak Container App')
param name string

@description('Location for the Container App')
param location string

@description('Container Apps Environment ID')
param environmentId string

@description('PostgreSQL FQDN')
param postgresFqdn string

@description('PostgreSQL admin username')
param postgresUsername string

@secure()
@description('PostgreSQL admin password')
param postgresPassword string

@secure()
@description('Keycloak administrator password')
param keycloakAdminPassword string

@description('Keycloak realm import JSON content')
param realmImportJson string = ''

var keycloakAdmin = 'admin'

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: name
  location: location
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      secrets: [
        {
          name: 'keycloak-admin-password'
          value: keycloakAdminPassword
        }
        {
          name: 'postgres-password'
          value: postgresPassword
        }
        {
          name: 'realm-import-json'
          value: realmImportJson
        }
      ]
    }
    template: {
      containers: [
        {
          name: name
          image: 'quay.io/keycloak/keycloak:26.0'
          command: !empty(realmImportJson) ? [
            '/bin/sh'
            '-c'
            '/opt/keycloak/bin/kc.sh import --dir /opt/keycloak/data/import --override true && exec /opt/keycloak/bin/kc.sh start'
          ] : [
            '/opt/keycloak/bin/kc.sh'
            'start'
          ]
          env: [
            { name: 'KC_DB', value: 'postgres' }
            { name: 'KC_DB_URL', value: 'jdbc:postgresql://${postgresFqdn}:5432/keycloak?sslmode=require' }
            { name: 'KC_DB_USERNAME', value: postgresUsername }
            { name: 'KC_DB_PASSWORD', secretRef: 'postgres-password' }
            { name: 'KC_HTTP_PORT', value: '8080' }
            { name: 'KC_PROXY_HEADERS', value: 'xforwarded' }
            { name: 'KC_HOSTNAME_STRICT', value: 'false' }
            { name: 'KC_HTTP_ENABLED', value: 'true' }
            { name: 'KEYCLOAK_ADMIN', value: keycloakAdmin }
            { name: 'KEYCLOAK_ADMIN_PASSWORD', secretRef: 'keycloak-admin-password' }
            { name: 'REALM_CONFIG_HASH', value: uniqueString(realmImportJson) }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          volumeMounts: !empty(realmImportJson) ? [
            {
              mountPath: '/opt/keycloak/data/import'
              volumeName: 'realm-import'
            }
          ] : []
        }
      ]
      volumes: !empty(realmImportJson) ? [
        {
          name: 'realm-import'
          storageType: 'Secret'
          secrets: [
            {
              secretRef: 'realm-import-json'
              path: 'risk-management-realm.json'
            }
          ]
        }
      ] : []
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
output adminUsername string = keycloakAdmin
