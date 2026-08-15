# Architecture Overview

## Purpose

This document provides a concise overview of the OrderFlow Integration Platform architecture.

It explains the main components and the end-to-end order-processing flow. Detailed architectural decisions and rationale are documented separately in the Architecture Decision Records.

---

## System Context

OrderFlow accepts customer orders through an API and processes them asynchronously.

```text
External Client
      |
      v
Azure API Management
      |
      v
Order Intake Function
      |
      v
Application Layer
      |
      v
Azure Service Bus
      |
      v
Order Processor Function
      |
      +----> Idempotency Store
      |
      +----> Inventory Reservation
      |
      v
Azure Table Storage
```

The synchronous API flow is intentionally separated from fulfilment processing through Azure Service Bus.

This allows the API to respond quickly while downstream processing runs independently.

---

## Architectural Style

OrderFlow follows Clean Architecture and Ports & Adapters principles.

The main dependency direction is:

```text
Functions
   |
   v
Application
   |
   v
Domain
```

Infrastructure implements interfaces defined by the Application layer.

```text
Azure Function
     |
     v
Application Use Case
     |
     v
Application Port
     ^
     |
Infrastructure Adapter
     |
     v
Azure Service / Storage / External System
```

This keeps business and application logic independent from Azure SDK implementations.

Architecture tests enforce the main dependency boundaries.

---

## Main Components

### Order Intake Function

Provides the HTTP entry point for submitting orders.

Responsibilities include:

* request deserialization;
* transport validation;
* correlation handling;
* mapping requests to application commands;
* invoking the order submission use case.

### Application Layer

Contains application use cases and outbound abstractions.

Important examples include:

* `SubmitOrderHandler`;
* `ProcessOrderSubmittedHandler`;
* `IIntegrationEventPublisher`;
* `IProcessedMessageStore`;
* `IInventoryReservationService`;
* `IOrderProcessingResultStore`.

### Infrastructure Layer

Implements Application ports using infrastructure technologies.

Current adapters include:

* Azure Service Bus publisher;
* Azure Table Storage persistence;
* processed-message storage;
* processing-result storage;
* simulated inventory reservation.

### Order Processor Function

Consumes order events from Azure Service Bus and invokes the asynchronous processing use case.

It also restores correlation and distributed tracing context before processing.

### Azure API Management

Represents the external API gateway boundary.

The current infrastructure definition exposes:

```text
POST /orders
```

APIM policies are maintained as source-controlled configuration.

---

## Order Processing Flow

### Order Submission

```text
Client
  |
  | POST /orders
  v
API Management
  |
  v
Order Intake Function
  |
  v
SubmitOrderHandler
  |
  v
Service Bus
  |
  v
202 Accepted
```

The flow is:

1. A client submits an order.
2. The HTTP request is validated.
3. The request is mapped to a `SubmitOrderCommand`.
4. `SubmitOrderHandler` creates an `OrderSubmittedIntegrationEvent`.
5. The event is published to Azure Service Bus.
6. The client receives `202 Accepted`.

The client does not wait for inventory processing to complete.

### Asynchronous Processing

```text
Service Bus
    |
    v
Order Processor
    |
    v
Check Idempotency
    |
    v
Reserve Inventory
    |
    v
Store Processing Result
    |
    v
Mark Message Processed
```

The consumer checks whether the message has already been processed before executing the business flow.

Inventory reservation is currently implemented by a deterministic simulated adapter so the complete solution can run locally.

---

## Reliability

OrderFlow assumes that messages can be delivered more than once and that technical failures can occur.

The messaging flow therefore includes:

* deterministic Service Bus message IDs;
* Service Bus duplicate detection;
* persistent consumer-side idempotency;
* retry handling;
* dead-letter handling.

Business failures, such as insufficient inventory, are stored as valid processing outcomes.

Technical failures are allowed to propagate so Service Bus can apply retry and dead-letter behavior.

A DLQ inspection utility is available under:

```text
tools/OrderFlow.DlqInspector
```

---

## Observability

OrderFlow supports correlation and distributed tracing across the asynchronous boundary.

```text
HTTP Request
    |
    v
SubmitOrder
TraceId = X
    |
    v
Service Bus
traceparent
    |
    v
ProcessOrderSubmitted
TraceId = X
```

The platform currently uses:

* `X-Correlation-ID`;
* structured logging;
* OpenTelemetry;
* W3C trace propagation;
* custom producer and consumer Activities;
* optional Azure Monitor / Application Insights export.

This allows a single order to be followed across both Function applications.

---

## Persistence

Azure Table Storage is used for:

### ProcessedMessages

Stores processed message IDs for consumer idempotency.

### OrderProcessingResults

Stores the outcome of asynchronous order processing.

The local development environment uses Azurite for these storage capabilities.

---

## Local and Azure Environments

The architecture is designed for Azure but can run locally using:

* Azure Functions Core Tools;
* Azure Service Bus Emulator;
* Azurite;
* Docker Compose.

Azure infrastructure definitions are maintained with Bicep under:

```text
infra/bicep/
```

Current modules cover:

* Service Bus;
* Storage;
* API Management.

---

## Related Documentation

### Project Scope

* [`project-charter.md`](project-charter.md)

### Architecture Decisions

* [`ADR-001-local-first-development.md`](../decisions/ADR-001-local-first-development.md)
* [`ADR-002-dotnet-10-functions-isolated-worker.md`](../decisions/ADR-002-dotnet-10-functions-isolated-worker.md)
* [`ADR-003-clean-architecture-boundaries.md`](../decisions/ADR-003-clean-architecture-boundaries.md)

The ADRs explain why the main architectural choices were made.

This document focuses only on how the current system fits together.
