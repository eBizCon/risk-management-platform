@description('Name of the Database Seeder Job')
param name string

@description('Location for the Job')
param location string

@description('Container Apps Environment ID')
param environmentId string

@description('Container image to deploy')
param image string

@description('Container Registry server')
param registryServer string

@description('Container Registry username')
param registryUsername string

@secure()
@description('Container Registry password')
param registryPassword string

@secure()
@description('Customer database connection string')
param customerConnectionString string

@secure()
@description('Risk database connection string')
param riskConnectionString string

resource containerAppJob 'Microsoft.App/jobs@2023-05-01' = {
  name: name
  location: location
  properties: {
    environmentId: environmentId
    workloadProfileName: null
    configuration: {
      secrets: concat(
        !empty(registryServer) ? [
          {
            name: 'registry-password'
            value: registryPassword
          }
        ] : [],
        [
          {
            name: 'customer-connection'
            value: customerConnectionString
          }
          {
            name: 'risk-connection'
            value: riskConnectionString
          }
        ]
      )
      registries: !empty(registryServer) ? [
        {
          server: registryServer
          username: registryUsername
          passwordSecretRef: 'registry-password'
        }
      ] : []
      replicaTimeout: 600
      replicaRetryLimit: 2
      triggerType: 'Manual'
    }
    template: {
      containers: [
        {
          name: name
          image: image
          env: [
            {
              name: 'ConnectionStrings__CustomerConnection'
              secretRef: 'customer-connection'
            }
            {
              name: 'ConnectionStrings__RiskConnection'
              secretRef: 'risk-connection'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
    }
  }
}

output jobName string = containerAppJob.name
