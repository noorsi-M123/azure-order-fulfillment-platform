using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Infrastructure.Messaging;

namespace OrderFlow.Functions.OrderProcessor;

public sealed class ProcessOrderSubmitted
{
   
    private readonly ILogger<ProcessOrderSubmitted> _logger;

    public ProcessOrderSubmitted(
        ILogger<ProcessOrderSubmitted> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ProcessOrderSubmitted))]
    public void Run(
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

        _logger.LogInformation(
            "Order submitted event received. OrderId: {OrderId}, CustomerId: {CustomerId}, MessageId: {MessageId}",
            integrationEvent.OrderId,
            integrationEvent.CustomerId,
            message.MessageId);
    }
}