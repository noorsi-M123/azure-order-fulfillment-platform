param storageAccountName string
param location string

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2025-08-01' = {
  parent: storageAccount
  name: 'default'
}

resource processedMessagesTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2025-08-01' = {
  parent: tableService
  name: 'ProcessedMessages'
}

resource orderProcessingResultsTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2025-08-01' = {
  parent: tableService
  name: 'OrderProcessingResults'
}

output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
