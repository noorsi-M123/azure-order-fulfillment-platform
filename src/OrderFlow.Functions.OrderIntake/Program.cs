using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

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

builder.Build().Run();