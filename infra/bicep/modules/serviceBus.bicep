param namespaceName string
param location string
param queueName string = 'orders-submitted'

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2026-01-01' = {
  name: namespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

resource ordersSubmittedQueue 'Microsoft.ServiceBus/namespaces/queues@2026-01-01' = {
  parent: serviceBusNamespace
  name: queueName
  properties: {
    deadLetteringOnMessageExpiration: true
    defaultMessageTimeToLive: 'PT1H'
    duplicateDetectionHistoryTimeWindow: 'PT5M'
    lockDuration: 'PT1M'
    maxDeliveryCount: 5
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: true
    requiresSession: false
  }
}

output namespaceName string = serviceBusNamespace.name
output namespaceId string = serviceBusNamespace.id
output queueName string = ordersSubmittedQueue.name
