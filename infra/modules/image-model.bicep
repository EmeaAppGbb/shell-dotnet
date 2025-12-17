@description('The name of the AI Services account')
param aiServicesAccountName string

@description('The name of the deployment')
param deploymentName string

@description('The SKU name for the deployment')
param skuName string = 'GlobalStandard'

@description('The SKU capacity for the deployment')
param skuCapacity int = 1

param format string 
param modelName string 
param modelVersion string

resource aiAccount 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' existing = {
  name: aiServicesAccountName
}

resource imageDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-04-01-preview' = {
  parent: aiAccount
  name: deploymentName
  sku: {
    name: skuName
    capacity: skuCapacity
  }
  properties: {
    model: {
      format: format
      name: modelName
      version: modelVersion
    }
     
  }
}

output deploymentName string = imageDeployment.name
output deploymentId string = imageDeployment.id

