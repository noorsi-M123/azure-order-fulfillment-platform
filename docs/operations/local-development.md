# Local Development

## Purpose

This document explains how to run the OrderFlow Integration Platform locally.

The local environment mirrors the Azure architecture where practical, without requiring paid Azure resources.

## Prerequisites

Install:

* .NET 10 SDK
* Azure Functions Core Tools v4
* Docker Desktop
* Docker Compose
* Azure Storage Explorer

Verify:

```powershell
dotnet --version
func.cmd --version
docker --version
docker compose version
```

## Start Local Infrastructure

From the repository root:

```powershell
cd infra\servicebus
docker compose up -d
docker compose ps
cd ..\..
```

This starts:

```text
Azure Service Bus Emulator
Azurite
SQL dependency
```

## Local Configuration

Both Function applications require a local `local.settings.json`.

These files are excluded from source control.

### Order Intake

```text
src/OrderFlow.Functions.OrderIntake/local.settings.json
```

Required settings:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBus:ConnectionString": "<local-service-bus-connection>",
    "Messaging:OrdersSubmittedQueueName": "orders-submitted"
  }
}
```

### Order Processor

```text
src/OrderFlow.Functions.OrderProcessor/local.settings.json
```

Required settings:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnection": "<local-service-bus-connection>",
    "Messaging:OrdersSubmittedQueueName": "orders-submitted"
  }
}
```

## Build

From the repository root:

```powershell
dotnet build OrderFlow.slnx
```

## Start Order Processor

Open a terminal:

```powershell
cd src\OrderFlow.Functions.OrderProcessor
func.cmd start
```

Keep this terminal running.

## Start Order Intake

Open a second terminal:

```powershell
cd src\OrderFlow.Functions.OrderIntake
func.cmd start --port 7072
```

## Submit a Test Order

Send:

```http
POST http://localhost:7072/api/orders
Content-Type: application/json
X-Correlation-ID: local-test-001
```

Example body:

```json
{
  "orderId": "ORD-LOCAL-001",
  "customerId": "CUST-001",
  "items": [
    {
      "productId": "PROD-001",
      "quantity": 1,
      "unitPrice": 25.00,
      "currency": "EUR"
    }
  ]
}
```

A valid request returns:

```text
202 Accepted
```

The order is then processed asynchronously.

## Verify Processing

Use Azure Storage Explorer to inspect:

```text
ProcessedMessages
OrderProcessingResults
```

Typical processing statuses are:

```text
Completed
Failed
```

To simulate insufficient inventory, use a product ID starting with:

```text
OUT-OF-STOCK
```

## Dead-Letter Queue

Messages that repeatedly fail technical processing are eventually moved to the dead-letter queue.

The repository includes a read-only diagnostic tool:

```text
tools/OrderFlow.DlqInspector
```

## Stop Local Infrastructure

```powershell
cd infra\servicebus
docker compose down
```

Use:

```powershell
docker compose down -v
```

only when local emulator data should also be removed.

## Related Documentation

* [Architecture Overview](../architecture/architecture-overview.md)
* [Project Charter](../architecture/project-charter.md)
* [Architecture Decision Records](../decisions/)
