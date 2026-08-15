using Azure.Messaging.ServiceBus;
using Azure.Monitor.OpenTelemetry.Exporter;
using FluentValidation;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Observability;
using OrderFlow.Application.Orders.SubmitOrder;
using OrderFlow.Contracts.Orders;
using OrderFlow.Functions.OrderIntake.Validators;
using OrderFlow.Infrastructure.Messaging;


var builder =
    FunctionsApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(
    LogLevel.Information);

builder.ConfigureFunctionsWebApplication();

var openTelemetryBuilder =
    builder.Services
        .AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .WithTracing(tracing =>
        {
            tracing.AddSource(
                OrderFlowActivitySource.Name);

            tracing.AddConsoleExporter();
        });

var applicationInsightsConnectionString =
    builder.Configuration[
        "APPLICATIONINSIGHTS_CONNECTION_STRING"];

if (!string.IsNullOrWhiteSpace(
        applicationInsightsConnectionString))
{
    openTelemetryBuilder.UseAzureMonitorExporter(
        options =>
        {
            options.ConnectionString =
                applicationInsightsConnectionString;
        });
}

builder.Services
    .AddOptions<MessagingOptions>()
    .Bind(
        builder.Configuration.GetSection(
            MessagingOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.OrdersSubmittedQueueName),
        "Messaging:OrdersSubmittedQueueName must be configured.")
    .ValidateOnStart();

var serviceBusConnectionString =
    builder.Configuration[
        "ServiceBus:ConnectionString"]
    ?? throw new InvalidOperationException(
        "Service Bus connection string is not configured.");

builder.Services.AddSingleton(
    new ServiceBusClient(
        serviceBusConnectionString));

builder.Services.AddSingleton(
    TimeProvider.System);

builder.Services.AddScoped<
    IValidator<SubmitOrderRequest>,
    SubmitOrderRequestValidator>();

builder.Services.AddScoped<
    ISubmitOrderHandler,
    SubmitOrderHandler>();

builder.Services.AddScoped<
    IIntegrationEventPublisher,
    ServiceBusIntegrationEventPublisher>();

builder.Build().Run();