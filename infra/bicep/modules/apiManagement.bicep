param serviceName string
param location string
param publisherName string
param publisherEmail string
param backendUrl string

param apiName string = 'order-api'
param apiPath string = 'orders'

resource apiManagement 'Microsoft.ApiManagement/service@2024-05-01' = {
  name: serviceName
  location: location
  sku: {
    name: 'Developer'
    capacity: 1
  }
  properties: {
    publisherName: publisherName
    publisherEmail: publisherEmail
  }
}

resource orderApi 'Microsoft.ApiManagement/service/apis@2024-05-01' = {
  parent: apiManagement
  name: apiName
  properties: {
    displayName: 'Order API'
    path: apiPath
    protocols: [
      'https'
    ]
    serviceUrl: backendUrl
    subscriptionRequired: false
  }
}

resource submitOrderOperation 'Microsoft.ApiManagement/service/apis/operations@2024-05-01' = {
  parent: orderApi
  name: 'submit-order'
  properties: {
    displayName: 'Submit Order'
    method: 'POST'
    urlTemplate: '/'
    description: 'Accepts an order for asynchronous processing.'
    responses: [
      {
        statusCode: 202
        description: 'Order accepted for asynchronous processing.'
      }
      {
        statusCode: 400
        description: 'Invalid order request.'
      }
    ]
  }
}

resource orderApiPolicy 'Microsoft.ApiManagement/service/apis/policies@2024-05-01' = {
  parent: orderApi
  name: 'policy'
  properties: {
    format: 'rawxml'
    value: loadTextContent('../../apim/policies/order-api-policy.xml')
  }
}

output serviceName string = apiManagement.name
output serviceId string = apiManagement.id
output apiName string = orderApi.name
