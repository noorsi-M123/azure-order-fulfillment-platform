# OrderFlow Integration Platform

OrderFlow is a production-oriented, local-first integration platform that demonstrates enterprise Azure integration and backend development practices.

The platform simulates an order fulfilment process in which external sales channels submit orders through an API gateway. Orders are validated, persisted and processed asynchronously through message-driven workflows.

The solution is designed for Azure, while the complete development and demonstration environment can run locally without requiring paid Azure resources.

> **Project status:** Architecture and repository foundation.

## Purpose

This repository demonstrates practical experience with:

- event-driven architecture;
- asynchronous integration patterns;
- Azure Functions using the isolated worker model;
- Azure Service Bus queues and topics;
- Azure Storage;
- API gateway design;
- idempotent message processing;
- retries and dead-letter handling;
- distributed tracing and structured logging;
- Clean Architecture;
- infrastructure as code;
- automated testing;
- containerized local development;
- continuous integration.

The project is intended as a professional portfolio project for software companies, recruiters and freelance clients.

## Business Scenario

External sales channels submit customer orders to OrderFlow.

OrderFlow accepts the request, validates the contract, stores the order and publishes an asynchronous command. A separate processing component reserves inventory and updates the order status.

The API confirms acceptance immediately. Fulfilment continues asynchronously.

```text
External Channel
       |
       v
API Gateway
       |
       v
Order Intake Function
       |
       +----> Azure Storage
       |
       v
Service Bus Queue
       |
       v
Order Processing Function
       |
       +----> Processing Result
       |
       v
Integration Event Topic