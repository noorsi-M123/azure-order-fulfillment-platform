# ADR-002: Use .NET 10 LTS with Azure Functions Isolated Worker

## Status

Accepted

## Date

2026-08-01

## Context

OrderFlow requires a supported and maintainable runtime for HTTP-triggered and message-driven serverless workloads.

The solution must support:

- dependency injection;
- explicit startup configuration;
- structured logging;
- OpenTelemetry;
- Azure Service Bus triggers;
- local development;
- automated testing;
- separation between Function triggers and application logic.

Azure Functions offers an in-process model and an isolated worker model for C# applications.

The in-process model is approaching the end of its support lifecycle and does not provide the same level of control over the .NET application process.

OrderFlow is a new project and should use a current Long-Term Support runtime and Microsoft's strategic execution model for Azure Functions.

## Decision

OrderFlow will use:

| Concern | Decision |
|---|---|
| Runtime | .NET 10 LTS |
| Target framework | `net10.0` |
| Azure Functions runtime | Version 4 |
| Execution model | Isolated worker |
| Language | C# |
| Local tooling | Azure Functions Core Tools 4 |
| Dependency injection | Standard .NET dependency injection |
| Package management | Central package management |
| SDK management | `global.json` |

All Azure Functions projects will use the isolated worker model.

Function projects will act as transport adapters and composition roots. Business logic will remain in the Application and Domain layers.

Domain and Application projects must not reference Azure Functions packages or trigger-specific types.

Package versions will be managed centrally through:

```text
Directory.Packages.props
```

The approved .NET SDK version will be declared through:

```text
global.json
```

## Rationale

This decision provides:

- a current LTS runtime;
- a longer support horizon;
- alignment with Microsoft's direction for Azure Functions;
- standard .NET dependency injection and configuration;
- explicit application startup;
- middleware support;
- improved testability;
- separation between the Functions host and application code;
- reduced future migration effort.

## Alternatives Considered

### Azure Functions In-Process Model

Rejected because:

- it is approaching the end of support;
- it is not appropriate for a new long-lived application;
- it provides less control over the application process;
- selecting it would introduce avoidable migration work.

### .NET 8 with Isolated Worker

Rejected because .NET 10 provides a longer support horizon and is the current LTS baseline for a new project.

### ASP.NET Core Web API as the Only Compute Model

Rejected because the project must demonstrate Azure Functions and event-driven serverless processing.

ASP.NET Core may still be used for the local API gateway.

### .NET Worker Service

Rejected as the primary processing model because Azure Functions is the intended serverless execution platform.

A Worker Service may be reconsidered if a future workload does not fit the Azure Functions execution model.

## Consequences

### Positive

- The project uses a current supported runtime.
- Function applications use Microsoft's strategic execution model.
- Standard .NET configuration and dependency injection are available.
- Function triggers can remain thin adapters.
- Domain and Application logic can be tested independently.
- The project avoids a future in-process migration.

### Negative

- Developers must understand the isolated worker model.
- Worker and trigger-extension packages must be managed explicitly.
- Some older Azure Functions examples are not applicable.
- Local and Azure-hosted behavior still require separate validation.

## Architectural Constraints

The following rules apply:

1. Every .NET project targets `net10.0`.
2. Every Function project uses the isolated worker model.
3. Domain and Application projects do not reference Azure Functions packages.
4. Function-specific types do not cross into the core layers.
5. Function methods remain thin and delegate to application use cases.
6. Preview packages require an explicit architectural decision.
7. Package and SDK versions are centrally controlled.

## Review Triggers

This ADR must be reviewed when:

- .NET 10 approaches end of support;
- a newer LTS version becomes the project baseline;
- Azure Functions changes support for .NET 10;
- a required binding is unavailable for isolated worker;
- a workload no longer fits the Azure Functions execution model.

## References

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [Azure Functions isolated worker process guide](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)
- [Azure Functions runtime versions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-versions)