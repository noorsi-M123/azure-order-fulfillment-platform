# ADR-001: Adopt a Zero-Cost Local-First Development Strategy

## Status

Accepted

## Date

2026-07-26

## Context

OrderFlow is a production-oriented portfolio project that demonstrates enterprise Azure integration and backend development practices.

The target architecture includes:

- Azure Functions;
- Azure Service Bus;
- Azure Storage;
- Azure API Management;
- Application Insights;
- infrastructure as code with Bicep.

Provisioning these resources in Azure can introduce recurring costs and requires an active Azure subscription. This creates unnecessary barriers for development, automated testing and evaluation by recruiters, clients and software engineers.

The project must remain representative of a production Azure architecture while allowing the complete development and demonstration workflow to run without paid cloud resources.

## Decision Drivers

The decision is based on the following requirements:

- local development must not generate Azure costs;
- contributors must not require an Azure subscription;
- the environment must be reproducible;
- Azure-specific behavior must remain visible in the architecture;
- application and domain logic must not depend directly on the hosting environment;
- infrastructure must remain deployable to Azure in the future;
- local development must support realistic messaging, persistence and observability;
- emulator limitations must be explicitly documented.

## Decision

OrderFlow will adopt a zero-cost, local-first development strategy.

The default development, testing and demonstration environment will run locally using Docker and locally hosted services.

The local environment will use:

- Azure Functions Core Tools for executing Azure Functions;
- the Azure Service Bus Emulator for queues, topics and dead-letter behavior;
- Azurite for Azure Blob and Table Storage;
- ASP.NET Core with YARP as the local API gateway;
- OpenTelemetry for traces, metrics and telemetry correlation;
- Aspire Dashboard for local observability;
- Docker Compose for infrastructure orchestration.

The production architecture will still be defined for Azure.

The repository will therefore also contain:

- Bicep modules for Azure infrastructure;
- Azure API Management API definitions;
- Azure API Management policies;
- environment-specific configuration;
- deployment documentation;
- security and operational guidance.

Deployment to Azure will remain optional and will not be required for normal development or continuous integration.

## Architectural Rules

The following rules apply to the implementation:

1. Domain logic must not reference Azure SDK packages.
2. Application use cases must not depend on Function triggers.
3. Infrastructure-specific implementations must be accessed through application ports.
4. Local and Azure implementations must use the same business contracts.
5. Environment-specific behavior must be configured through dependency injection and configuration.
6. Secrets and connection strings must not be committed to source control.
7. Emulator limitations must not be hidden behind inaccurate abstractions.
8. Bicep must be validated in continuous integration without deploying resources.
9. Azure deployment must not be added to the default CI workflow.
10. A real Azure acceptance test is required before the solution could be considered production-ready.

## Local and Azure Component Mapping

| Architectural capability | Local environment | Azure environment |
|---|---|---|
| Compute | Azure Functions Core Tools | Azure Functions |
| Messaging | Azure Service Bus Emulator | Azure Service Bus |
| Blob storage | Azurite | Azure Blob Storage |
| Table storage | Azurite | Azure Table Storage |
| API gateway | ASP.NET Core and YARP | Azure API Management |
| Tracing and metrics | OpenTelemetry and Aspire Dashboard | Application Insights and Azure Monitor |
| Infrastructure orchestration | Docker Compose | Bicep and Azure Resource Manager |
| Continuous integration | GitHub Actions | GitHub Actions |
| Deployment | Not required | Optional environment workflow |

## Alternatives Considered

### Alternative 1: Permanently Deploy All Components to Azure

This option would provide the highest level of cloud-platform fidelity.

It was rejected as the default approach because:

- it introduces recurring costs;
- it requires Azure credentials;
- it creates a barrier for external reviewers;
- it complicates local development;
- it increases the risk of accidentally leaving resources active.

Azure deployment remains available as an optional future capability.

### Alternative 2: Replace Azure Services with Unrelated Local Technologies

Examples include:

- RabbitMQ instead of Azure Service Bus;
- PostgreSQL instead of Azure Storage;
- a generic web API instead of Azure Functions.

This option was rejected because it would weaken the project’s purpose as an Azure integration portfolio.

Local infrastructure should remain as close as reasonably possible to the Azure target services.

### Alternative 3: Use Only In-Memory Implementations

This option would simplify unit testing and local startup.

It was rejected as the primary development environment because in-memory implementations do not adequately demonstrate:

- message delivery;
- dead-letter behavior;
- concurrent processing;
- persistence;
- container orchestration;
- infrastructure failures;
- integration testing.

In-memory implementations may still be used in isolated unit tests where appropriate.

### Alternative 4: Depend on Azure Free Allowances

This option could reduce costs while using managed Azure services.

It was rejected because:

- free allowances can change;
- some services still have fixed costs;
- payment details may be required;
- incorrect configuration can create unexpected charges;
- the project would no longer guarantee a zero-cost development workflow.

## Consequences

### Positive Consequences

- The project can be developed without Azure costs.
- Reviewers can run the solution without an Azure subscription.
- The local environment can be recreated through Docker Compose.
- Application and domain logic remain independent from the hosting environment.
- Azure infrastructure remains visible and reviewable through Bicep.
- Integration tests can run against realistic infrastructure.
- The project demonstrates cost-aware cloud engineering.

### Negative Consequences

- Local emulators do not support every managed Azure capability.
- Managed identity cannot be fully validated locally.
- Azure networking and private endpoints cannot be reproduced completely.
- Azure platform scaling behavior cannot be tested locally.
- API Management behavior must be represented by both a local gateway and APIM policy definitions.
- A final Azure-hosted acceptance test would still be required before production use.

## Risks and Mitigations

### Risk: Emulator Behavior Differs from Azure

Local behavior may differ from managed Azure services.

Mitigations:

- isolate Azure integrations in infrastructure adapters;
- use official SDKs and message contracts;
- document unsupported emulator features;
- avoid relying on undocumented emulator behavior;
- maintain optional Azure acceptance-test scenarios.

### Risk: Configuration Drift

Local and Azure configurations may evolve independently.

Mitigations:

- centralize configuration naming conventions;
- validate Bicep in GitHub Actions;
- document environment differences;
- keep local and Azure settings structurally aligned;
- review configuration changes as part of pull requests.

### Risk: Local Gateway Diverges from API Management Policies

YARP middleware and APIM policies may implement different behavior.

Mitigations:

- define the API contract in OpenAPI;
- document the responsibility of each gateway policy;
- create contract tests for externally visible behavior;
- treat APIM policy files as production artifacts;
- avoid implementing business rules in either gateway.

### Risk: Portfolio Claims Exceed Implemented Capabilities

Documentation may suggest that Azure production behavior has been validated when only local emulators were used.

Mitigations:

- clearly distinguish designed, implemented and Azure-validated capabilities;
- document emulator limitations;
- avoid claiming production deployment until Azure acceptance tests have been executed.

## Operational Impact

Developers must be able to start the required infrastructure through a documented Docker Compose command.

The project must provide:

- a local startup guide;
- health checks;
- container logs;
- deterministic configuration;
- teardown instructions;
- troubleshooting guidance.

Local runtime data must not be committed to Git.

## Security Impact

The local-first strategy does not remove production security requirements.

The Azure design must still include:

- managed identities;
- least-privilege role assignments;
- secret management;
- transport security;
- API authentication;
- authorization;
- diagnostic settings;
- secure configuration.

Local authentication substitutes must be clearly identified as development-only behavior.

## Compliance

This decision supports the following engineering principles:

- separation of concerns;
- dependency inversion;
- infrastructure portability;
- reproducible environments;
- cost governance;
- testability;
- explicit operational constraints.

## Review Triggers

This ADR must be reviewed when:

- the project requires a permanently hosted demonstration environment;
- a real client or production deployment becomes part of the scope;
- an emulator cannot support a critical requirement;
- Azure acceptance testing becomes mandatory;
- infrastructure costs become acceptable through sponsorship or budget;
- the selected local Azure emulator is deprecated or no longer maintained.

## Outcome

The default OrderFlow development workflow will remain free of Azure cloud costs.

The system will be developed locally using Azure-compatible emulators and production-oriented abstractions, while Azure deployment artifacts remain part of the repository.