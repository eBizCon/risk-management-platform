@description('Name of the Container Registry')
param name string

@description('Location for the Container Registry')
param location string

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: name
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

output loginServer string = acr.properties.loginServer
output name string = acr.name
output adminUsername string = acr.listCredentials().username
output adminPassword string = acr.listCredentials().passwords[0].value
