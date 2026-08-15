# OrderFlow Integration Platform

OrderFlow is a production-oriented, local-first integration platform demonstrating enterprise Azure integration and backend engineering practices.

The platform accepts customer orders through an API boundary and processes them asynchronously using Azure Functions, Azure Service Bus and Azure Storage. Reliability, observability, idempotency, infrastructure-as-code and automated quality controls are first-class concerns.

The complete development environment can run locally without requiring paid Azure resources.

> **Status:** Core implementation, reliability, observability, infrastructure-as-code, CI and automated testing are implemented.

## Architecture Overview

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
      +----> Idempotency
      |
      +----> Inventory Reservation
      |
      v
Azure Table Storage
```

The solution follows Clean Architecture and Ports & Adapters principles.

Detailed architectural decisions are documented separately under [`docs/`](docs/).

## Key Capabilities

* asynchronous order processing;
* Azure Functions isolated worker;
* Azure Service Bus messaging;
* persistent consumer idempotency;
* broker duplicate detection;
* retry and dead-letter handling;
* simulated inventory integration;
* Azure Table Storage persistence;
* correlation IDs;
* OpenTelemetry distributed tracing;
* API Management policy-as-code;
* Bicep infrastructure definitions;
* deterministic time handling with `TimeProvider`;
* unit, architecture and integration testing;
* GitHub Actions CI;
* protected `main` branch and pull-request workflow.

## Technology Stack

| Area              | Technology                         |
| ----------------- | ---------------------------------- |
| Runtime           | .NET 10, C#                        |
| Compute           | Azure Functions v4 Isolated Worker |
| Messaging         | Azure Service Bus                  |
| Storage           | Azure Table Storage / Azurite      |
| API Gateway       | Azure API Management               |
| Validation        | FluentValidation                   |
| Observability     | OpenTelemetry, Azure Monitor       |
| Infrastructure    | Bicep                              |
| Local environment | Docker Compose                     |
| Testing           | xUnit, ArchUnitNET                 |
| CI                | GitHub Actions                     |

## Solution Structure

```text
src/
├── OrderFlow.Domain
├── OrderFlow.Application
├── OrderFlow.Contracts
├── OrderFlow.Infrastructure
├── OrderFlow.Functions.OrderIntake
└── OrderFlow.Functions.OrderProcessor

tests/
├── OrderFlow.Domain.UnitTests
├── OrderFlow.Application.UnitTests
├── OrderFlow.Functions.OrderIntake.UnitTests
├── OrderFlow.ArchitectureTests
└── OrderFlow.IntegrationTests

infra/
├── apim/
├── bicep/
└── servicebus/

docs/
├── architecture/
└── decisions/

tools/
└── OrderFlow.DlqInspector
```

## Order Processing Flow

1. A client submits an order.
2. The HTTP contract is validated.
3. `SubmitOrderHandler` publishes an `OrderSubmittedIntegrationEvent`.
4. Azure Service Bus buffers the event.
5. `OrderProcessor` receives the message asynchronously.
6. The consumer checks whether the message was already processed.
7. Inventory reservation is executed through an application port.
8. The processing result is stored in Azure Table Storage.
9. The message is marked as processed.

Business failures such as insufficient inventory are persisted as valid processing outcomes.

Technical failures are allowed to propagate so Service Bus retry and DLQ behavior remains effective.

## Reliability

The asynchronous flow includes:

* deterministic message IDs;
* Service Bus duplicate detection;
* persistent consumer-side idempotency;
* retry handling;
* dead-letter handling;
* dedicated DLQ inspection tooling.

The queue configuration is defined in both the local emulator setup and Bicep infrastructure definitions.

## Observability

OrderFlow implements correlation and tracing across the complete asynchronous flow.

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

Observability includes:

* `X-Correlation-ID`;
* structured logging scopes;
* OpenTelemetry;
* W3C distributed trace propagation;
* consumer spans using `ActivityKind.Consumer`;
* optional Azure Monitor / Application Insights export.

## Security and Configuration

Runtime configuration is externalized rather than hardcoded.

Examples include:

* Service Bus connection configuration;
* messaging queue names;
* environment-specific Bicep parameters.

Incoming correlation IDs are validated before being propagated through logs and messaging metadata.

Sensitive local settings are excluded from source control.

## Testing

The repository contains multiple test levels:

* domain unit tests;
* application unit tests;
* Function transport tests;
* architecture tests;
* integration tests.

Architecture tests enforce dependency boundaries between Domain, Application, Infrastructure, Contracts and Function adapters.

Integration tests verify order-processing behavior using the real simulated inventory adapter.

Run all tests with:

```powershell
dotnet test OrderFlow.slnx
```

## Local Development

The project is designed to run locally.

Required tooling:

* .NET 10 SDK;
* Azure Functions Core Tools;
* Docker Desktop;
* Azure CLI;
* Bicep CLI.

Local infrastructure includes:

* Azure Service Bus Emulator;
* Azurite.

Build the solution:

```powershell
dotnet build OrderFlow.slnx
```

Start the local infrastructure from:

```text
infra/servicebus/
```

The Function applications can then be started independently with Azure Functions Core Tools.

Local connection strings and settings are intentionally not committed.

## Infrastructure as Code

Azure infrastructure is defined in:

```text
infra/bicep/
```

Current modules cover:

* Azure Service Bus;
* the `orders-submitted` queue;
* Azure Storage;
* Table Storage tables;
* Azure API Management;
* the Order API.

Environment-specific values live under:

```text
infra/bicep/environments/
```

Validate the main template with:

```powershell
az bicep build --file infra\bicep\main.bicep
```

## CI and Repository Governance

GitHub Actions runs build and tests on pushes and pull requests.

The `main` branch is protected.

Development follows:

```text
feature branch
      |
      v
Pull Request
      |
      v
Build and test
      |
      v
Merge
      |
      v
main
```

Direct pushes to `main` are blocked.

## Documentation

Detailed design information is intentionally kept outside this README.

### Architecture

* [Project Charter](docs/architecture/project-charter.md)

### Architecture Decision Records

* [ADR-001 — Local-first development](docs/decisions/ADR-001-local-first-development.md)
* [ADR-002 — .NET 10 and Azure Functions isolated worker](docs/decisions/ADR-002-dotnet-10-functions-isolated-worker.md)
* [ADR-003 — Clean Architecture boundaries](docs/decisions/ADR-003-clean-architecture-boundaries.md)

These documents contain the detailed rationale, alternatives, constraints and consequences behind the architecture.

## Known Production Considerations

OrderFlow is production-oriented, but the repository intentionally remains local-first.

A real production deployment should additionally evaluate:

* Managed Identity;
* Azure Key Vault;
* private networking;
* atomic inbox/outbox processing;
* downstream idempotency guarantees;
* centralized alerting;
* environment-specific deployment pipelines;
* workload-specific scaling and storage partitioning.

## Project Goal

OrderFlow is designed as a professional portfolio implementation rather than a tutorial application.

The goal is to demonstrate practical enterprise integration engineering: clear architecture, asynchronous communication, resilience, observability, testability, infrastructure-as-code and disciplined delivery practices.
