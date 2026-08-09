using Azure.Data.Tables;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.ProcessOrderSubmitted;
using OrderFlow.Infrastructure.Messaging;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Orders.Processing;
using OrderFlow.Infrastructure.Inventory;
using OrderFlow.Infrastructure.Orders.Processing;
using OrderFlow.Application.Observability;
using OpenTelemetry.Trace;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var openTelemetryBuilder = builder.Services
    .AddOpenTelemetry()
    .UseFunctionsWorkerDefaults()
    .WithTracing(tracing =>
    {
        tracing.AddSource(OrderFlowActivitySource.Name);
        tracing.AddConsoleExporter();
    });
    
var applicationInsightsConnectionString =
    builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    openTelemetryBuilder.UseAzureMonitorExporter(options =>
    {
        options.ConnectionString = applicationInsightsConnectionString;
    });
}

var storageConnectionString =
    builder.Configuration["AzureWebJobsStorage"]
    ?? throw new InvalidOperationException(
        "Azure Storage connection string is not configured.");

var tableServiceClient =
    new TableServiceClient(storageConnectionString);

var processedMessagesTable =
    tableServiceClient.GetTableClient("ProcessedMessages");

await processedMessagesTable.CreateIfNotExistsAsync();

var orderProcessingResultsTable =
    tableServiceClient.GetTableClient("OrderProcessingResults");

await orderProcessingResultsTable.CreateIfNotExistsAsync();

builder.Services.AddScoped<IInventoryReservationService,SimulatedInventoryReservationService>();
builder.Services.AddScoped<IOrderProcessingResultStore,TableOrderProcessingResultStore>();
builder.Services.AddSingleton(tableServiceClient);
builder.Services.AddScoped<IProcessedMessageStore, TableProcessedMessageStore>();
builder.Services.AddScoped<ProcessOrderSubmittedHandler>();
builder.Build().Run();