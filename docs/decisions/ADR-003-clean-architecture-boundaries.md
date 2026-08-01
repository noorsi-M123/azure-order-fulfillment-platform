# ADR-003: Define Clean Architecture Boundaries

## Status

Accepted

## Date

2026-08-01

## Context

OrderFlow contains HTTP triggers, Service Bus triggers, business rules, persistence and Azure-specific infrastructure.

Without clear boundaries, business logic can become mixed with Azure Functions, Service Bus and storage code. This would make the solution harder to test, maintain and extend.

The architecture must keep business logic independent from hosting and infrastructure technologies.

## Decision

OrderFlow will use the following projects:

| Project | Responsibility |
|---|---|
| `OrderFlow.Domain` | Business entities, value objects, invariants and domain behavior |
| `OrderFlow.Application` | Use cases, orchestration and application-owned interfaces |
| `OrderFlow.Contracts` | External API and messaging contracts |
| `OrderFlow.Infrastructure` | Azure Storage, Service Bus and external-system implementations |
| `OrderFlow.Functions.OrderIntake` | HTTP-triggered adapter and composition root |
| `OrderFlow.Functions.OrderProcessing` | Service Bus-triggered adapter and composition root |
| `OrderFlow.Gateway` | Local API gateway |

The project dependencies will be:

```text
Functions ─────────────► Application ─────────────► Domain
    │                        ▲
    │                        │
    └────────────► Infrastructure
                             │
                             └── implements interfaces
                                 defined by Application

Functions ─────────────► Contracts
Gateway ───────────────► Contracts
```

The runtime flow for an incoming request will be:

```text
Trigger
  |
  v
Function adapter
  |
  v
Application use case
  |
  +---------------------> Domain behavior
  |
  +---------------------> Application port
                              |
                              v
                      Infrastructure adapter
                              |
                              v
                       External technology
```

The Function projects reference Infrastructure only because they are composition roots. They register concrete Infrastructure implementations for interfaces defined by Application.

Function classes must not call persistence or messaging implementations directly.

## Layer Responsibilities

### Domain

Contains:

- entities;
- value objects;
- business rules;
- invariants;
- state transitions.

Domain must not reference Azure, Functions, HTTP, Service Bus or persistence packages.

### Application

Contains:

- use cases;
- commands and queries;
- application results;
- interfaces for persistence, messaging and external systems.

Application may reference Domain, but must not reference Infrastructure or Azure SDKs.

### Contracts

Contains:

- API request and response models;
- Service Bus message contracts;
- integration events;
- contract versions.

Contracts are not domain entities or persistence models.

### Infrastructure

Contains concrete implementations for:

- Azure Table Storage;
- Azure Blob Storage;
- Azure Service Bus;
- downstream inventory integration;
- idempotency storage.

Infrastructure implements interfaces defined by Application.

### Functions

Functions are thin inbound adapters.

They are responsible for:

- receiving requests or messages;
- reading headers and metadata;
- mapping contracts;
- invoking Application use cases;
- mapping results to HTTP responses or message settlement.

They must not contain business rules.

### Gateway

The local gateway may handle:

- authentication simulation;
- rate limiting;
- correlation headers;
- request-size limits;
- forwarding.

It must not contain order business logic.

## Repository Pattern

Repositories will only be used for meaningful persistence abstractions.

Appropriate examples:

```text
IOrderRepository
IIdempotencyRepository
IProcessingRecordRepository
```

The following abstractions are not allowed:

```text
IGenericRepository<T>
IServiceBusRepository
ILoggerRepository
IConfigurationRepository
```

Messaging abstractions must describe intent, for example:

```text
IOrderCommandPublisher
IIntegrationEventPublisher
```

## Architectural Rules

1. Domain references no other OrderFlow project.
2. Application references only Domain.
3. Infrastructure may reference Application and Domain.
4. Function projects may reference Application, Infrastructure and Contracts.
5. Gateway may reference Contracts, but not Domain.
6. Application interfaces must not expose Azure SDK types.
7. API contracts, domain models and persistence models remain separate.
8. Function classes delegate business processing to Application use cases.
9. Circular project references are prohibited.
10. These rules will be enforced with architecture tests.

## Rationale

This structure provides:

- testable business logic;
- thin Azure Function adapters;
- isolated Azure SDK usage;
- replaceable infrastructure implementations;
- explicit external contracts;
- clear dependency direction;
- maintainable project boundaries.

## Alternatives Considered

### One Azure Functions Project with All Logic

Rejected because it would mix transport, business and infrastructure concerns.

### Generic Repository Pattern

Rejected because generic CRUD interfaces hide business intent and do not fit every storage requirement.

### Direct Azure SDK Usage in Application

Rejected because it would couple use cases to Azure and reduce testability.

### More Projects and Layers

Rejected because additional boundaries would add complexity without a clear responsibility.

## Consequences

### Positive

- Business logic can be tested without Azure Functions or Docker.
- Azure SDK usage remains isolated.
- External contracts can evolve independently.
- Infrastructure can be replaced without changing Domain.
- Dependency rules can be validated automatically.

### Negative

- Mapping between contracts, domain and storage models is required.
- Simple features may touch multiple projects.
- Poorly designed interfaces can create unnecessary abstraction.

## Validation

This decision is correctly implemented when:

- project references follow the documented diagram;
- Domain and Application contain no Azure package references;
- Function-specific types do not appear in core-layer APIs;
- Functions invoke Application use cases instead of Infrastructure directly;
- architecture tests detect prohibited dependencies.

## Review Triggers

Review this ADR when:

- a new deployable workload is introduced;
- a new project boundary is proposed;
- direct Azure SDK usage is requested in Application;
- a generic repository is proposed;
- the solution is split into separately owned services.

## References

- [Dependency inversion principle](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles#dependency-inversion)
- [.NET application architecture guidance](https://learn.microsoft.com/en-us/dotnet/architecture/)