using '../main.bicep'

param location = 'westeurope'

param serviceBusNamespaceName = 'sb-orderflow-dev'
param storageAccountName = 'storderflowdev'

param apiManagementServiceName = 'apim-orderflow-dev'
param apiManagementPublisherName = 'OrderFlow'
param apiManagementPublisherEmail = 'developer@example.com'
param orderIntakeBackendUrl = 'https://orderflow-dev.example.com/api/orders'
