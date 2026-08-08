using Azure.Monitor.OpenTelemetry.Exporter;
using FluentValidation;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.SubmitOrder;
using OrderFlow.Contracts.Orders;
using OrderFlow.Functions.OrderIntake.Validators;
using OrderFlow.Infrastructure.Messaging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.ConfigureFunctionsWebApplication();

var openTelemetryBuilder = builder.Services
    .AddOpenTelemetry()
    .UseFunctionsWorkerDefaults();

var applicationInsightsConnectionString =
    builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    openTelemetryBuilder.UseAzureMonitorExporter(options =>
    {
        options.ConnectionString = applicationInsightsConnectionString;
    });
}

builder.Services.AddScoped<IValidator<SubmitOrderRequest>, SubmitOrderRequestValidator>();
builder.Services.AddScoped<ISubmitOrderHandler, SubmitOrderHandler>();
builder.Services.AddScoped<IIntegrationEventPublisher, LoggingIntegrationEventPublisher>();

builder.Build().Run();