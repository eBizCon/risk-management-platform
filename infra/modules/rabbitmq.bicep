@description('Name of the RabbitMQ Container App')
param name string

@description('Location for the Container App')
param location string

@description('Container Apps Environment ID')
param environmentId string

@secure()
@description('RabbitMQ administrator password')
param adminPassword string

var rabbitmqUsername = 'risk'

resource containerApp 'Microsoft.App/containerApps@2026-01-01' = {
  name: name
  location: location
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 15672
        transport: 'auto'
        allowInsecure: false
        additionalPortMappings: [
          {
            external: false
            targetPort: 5672
          }
        ]
      }
      secrets: [
        {
          name: 'rabbitmq-password'
          value: adminPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: name
          image: 'docker.io/rabbitmq:4-management'
          env: [
            { name: 'RABBITMQ_DEFAULT_USER', value: rabbitmqUsername }
            { name: 'RABBITMQ_DEFAULT_PASS', secretRef: 'rabbitmq-password' }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        cooldownPeriod: 3600
        pollingInterval: 30
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
output internalName string = name
output username string = rabbitmqUsername
