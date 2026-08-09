using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
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

        var result = await _handler.HandleAsync(
            message.MessageId,
            integrationEvent);

        if (result == ProcessOrderSubmittedResult.AlreadyProcessed)
        {
            _logger.LogInformation(
                "Message already processed. Skipping duplicate. MessageId: {MessageId}, OrderId: {OrderId}",
                message.MessageId,
                integrationEvent.OrderId);

            return;
        }

        _logger.LogInformation(
            "Order submitted event processed. OrderId: {OrderId}, CustomerId: {CustomerId}, MessageId: {MessageId}",
            integrationEvent.OrderId,
            integrationEvent.CustomerId,
            message.MessageId);
    }
}