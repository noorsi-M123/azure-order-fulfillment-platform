targetScope = 'resourceGroup'

@description('Azure region in which OrderFlow resources are deployed.')
param location string = resourceGroup().location

@description('Globally unique Service Bus namespace name.')
param serviceBusNamespaceName string

@description('Globally unique Azure Storage account name.')
param storageAccountName string

module serviceBus './modules/serviceBus.bicep' = {
  name: 'service-bus'
  params: {
    namespaceName: serviceBusNamespaceName
    location: location
  }
}

module storage './modules/storage.bicep' = {
  name: 'storage'
  params: {
    storageAccountName: storageAccountName
    location: location
  }
}
