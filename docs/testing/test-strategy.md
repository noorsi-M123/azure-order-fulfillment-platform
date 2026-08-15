# Test Strategy

## Purpose

This document describes the automated testing approach used in the OrderFlow Integration Platform.

The goal is to verify business behavior, application orchestration, architectural boundaries, and selected integration flows without over-testing implementation details.

## Test Levels

OrderFlow uses multiple test levels.

### Domain Unit Tests

Validate domain behavior independently from infrastructure and Azure services.

### Application Unit Tests

Validate application use cases using controlled test doubles.

Current scenarios include:

* order submission;
* integration-event creation;
* successful order processing;
* failed inventory reservation;
* duplicate-message handling;
* deterministic timestamps using `TimeProvider`.

### Function Unit Tests

Validate transport-specific Function behavior.

Current coverage includes correlation ID handling such as:

* valid correlation ID;
* missing correlation ID;
* unsafe characters;
* excessive length.

### Architecture Tests

ArchUnitNET verifies dependency rules between projects.

Examples include:

* Domain remains infrastructure-independent;
* Application does not depend on Infrastructure;
* Contracts remain independent;
* architectural boundaries cannot be bypassed accidentally.

### Integration Tests

Integration tests combine multiple real application components.

Current scenarios verify:

```text
OrderSubmittedIntegrationEvent
        |
        v
ProcessOrderSubmittedHandler
        |
        v
SimulatedInventoryReservationService
        |
        v
Processing Result
```

Both successful and insufficient-inventory scenarios are covered.

## Test Principles

The project follows these principles:

* test observable behavior rather than implementation details;
* keep business logic independent from Azure SDKs;
* use deterministic time through `TimeProvider`;
* prefer real lightweight adapters where practical;
* use architecture tests to protect dependency boundaries;
* keep external infrastructure integration separate from unit tests.

## Running Tests

From the repository root:

```powershell
dotnet test OrderFlow.slnx
```

The same test suite is executed by GitHub Actions as part of the CI pipeline.

## Current Scope

The automated test suite currently covers the core application flow and architecture.

Full emulator-based end-to-end tests against Service Bus and Azurite are not yet part of the automated CI pipeline.

Those flows are currently validated through local integration and manual end-to-end testing.

## Related Documentation

* [Architecture Overview](../architecture/architecture-overview.md)
* [Local Development](../operations/local-development.md)
* [Architecture Decision Records](../decisions/)
