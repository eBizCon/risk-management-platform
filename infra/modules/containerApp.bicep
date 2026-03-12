@description('Name of the Container App')
param name string

@description('Location for the Container App')
param location string

@description('Container Apps Environment ID')
param environmentId string

@description('Container image to deploy')
param image string

@description('Environment variables for the container')
param envVars array = []

@description('Ingress target port')
param ingressPort int

@description('Whether ingress is external')
param ingressExternal bool = true

@description('Minimum number of replicas')
param minReplicas int = 0

@description('Maximum number of replicas')
param maxReplicas int = 5

@description('Container Registry server')
param registryServer string = ''

@description('Container Registry username')
param registryUsername string = ''

@secure()
@description('Container Registry password')
param registryPassword string = ''

@description('Container command override')
param command array = []

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: name
  location: location
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      ingress: {
        external: ingressExternal
        targetPort: ingressPort
        transport: 'auto'
        allowInsecure: false
      }
      registries: !empty(registryServer) ? [
        {
          server: registryServer
          username: registryUsername
          passwordSecretRef: 'registry-password'
        }
      ] : []
      secrets: !empty(registryServer) ? [
        {
          name: 'registry-password'
          value: registryPassword
        }
      ] : []
    }
    template: {
      containers: [
        {
          name: name
          image: image
          env: envVars
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          command: !empty(command) ? command : null
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
