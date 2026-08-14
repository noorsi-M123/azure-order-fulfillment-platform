targetScope = 'resourceGroup'

@description('Azure region in which OrderFlow resources are deployed.')
param location string = resourceGroup().location

@description('Globally unique Service Bus namespace name.')
param serviceBusNamespaceName string

@description('Globally unique Azure Storage account name.')
param storageAccountName string

@description('Globally unique API Management service name.')
param apiManagementServiceName string

@description('API Management publisher display name.')
param apiManagementPublisherName string

@description('API Management publisher contact email.')
param apiManagementPublisherEmail string

@description('Backend URL used by API Management for the Order Intake Function.')
param orderIntakeBackendUrl string

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

module apiManagement './modules/apiManagement.bicep' = {
  name: 'api-management'
  params: {
    serviceName: apiManagementServiceName
    location: location
    publisherName: apiManagementPublisherName
    publisherEmail: apiManagementPublisherEmail
    backendUrl: orderIntakeBackendUrl
  }
}
