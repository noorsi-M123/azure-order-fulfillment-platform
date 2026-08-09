using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Observability;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Application.Orders.ProcessOrderSubmitted;
using OrderFlow.Infrastructure.Messaging;

namespace OrderFlow.Functions.OrderProcessor;

public sealed class ProcessOrderSubmitted
{
    private readonly ILogger<ProcessOrderSubmitted> _logger;
    private readonly ProcessOrderSubmittedHandler _handler;

    public ProcessOrderSubmitted(
        ILogger<ProcessOrderSubmitted> logger,
        ProcessOrderSubmittedHandler handler)
    {
        _logger = logger;
        _handler = handler;
    }

    [Function(nameof(ProcessOrderSubmitted))]
    public async Task Run(
        [ServiceBusTrigger(
            "orders-submitted",
            Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        var integrationEvent =
            message.Body.ToObjectFromJson<OrderSubmittedIntegrationEvent>(
                IntegrationEventJsonSerializer.Options);

        if (integrationEvent is null)
        {
            throw new InvalidOperationException(
                "The received Service Bus message could not be deserialized to OrderSubmittedIntegrationEvent.");
        }

        var correlationId =
            message.CorrelationId
            ?? integrationEvent.CorrelationId;

        using var activity =
            OrderFlowActivitySource.Instance.StartActivity(
                "ProcessOrderSubmitted");

        activity?.SetTag(
            "orderflow.correlation_id",
            correlationId);

        activity?.SetTag(
            "orderflow.order_id",
            integrationEvent.OrderId);

        activity?.SetTag(
            "orderflow.customer_id",
            integrationEvent.CustomerId);

        activity?.SetTag(
            "messaging.message.id",
            message.MessageId);

        activity?.SetTag(
            "messaging.destination.name",
            "orders-submitted");

        using var loggingScope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["OrderId"] = integrationEvent.OrderId,
                ["CustomerId"] = integrationEvent.CustomerId,
                ["MessageId"] = message.MessageId,
                ["CorrelationId"] = correlationId
            });

        _logger.LogInformation(
            "Processing order submitted event.");

        var result = await _handler.HandleAsync(
            message.MessageId,
            integrationEvent);

        if (result == ProcessOrderSubmittedResult.AlreadyProcessed)
        {
            activity?.SetTag(
                "orderflow.processing.result",
                "already_processed");

            _logger.LogInformation(
                "Message already processed. Skipping duplicate.");

            return;
        }

        activity?.SetTag(
            "orderflow.processing.result",
            "processed");

        _logger.LogInformation(
            "Order submitted event processed successfully.");
    }
}