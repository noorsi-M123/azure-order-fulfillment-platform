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

var builder = FunctionsApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

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

var storageConnectionString =
    builder.Configuration["AzureWebJobsStorage"]
    ?? throw new InvalidOperationException(
        "Azure Storage connection string is not configured.");

var tableServiceClient =
    new TableServiceClient(storageConnectionString);

var processedMessagesTable =
    tableServiceClient.GetTableClient("ProcessedMessages");

await processedMessagesTable.CreateIfNotExistsAsync();

builder.Services.AddSingleton(tableServiceClient);
builder.Services.AddScoped<IProcessedMessageStore, TableProcessedMessageStore>();
builder.Services.AddScoped<ProcessOrderSubmittedHandler>();
builder.Build().Run();