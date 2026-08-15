using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Infrastructure.Messaging;

public sealed class ServiceBusIntegrationEventPublisher
    : IIntegrationEventPublisher
{
    private const string TraceParentPropertyName = "traceparent";

    private readonly ServiceBusSender _sender;

    public ServiceBusIntegrationEventPublisher(
        ServiceBusClient serviceBusClient,
        IOptions<MessagingOptions> messagingOptions)
    {
        ArgumentNullException.ThrowIfNull(serviceBusClient);
        ArgumentNullException.ThrowIfNull(messagingOptions);

        var queueName =
            messagingOptions.Value.OrdersSubmittedQueueName;

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InvalidOperationException(
                "Messaging:OrdersSubmittedQueueName is not configured.");
        }

        _sender =
            serviceBusClient.CreateSender(queueName);
    }

    public async Task PublishAsync<T>(
        T integrationEvent,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var body = BinaryData.FromObjectAsJson(
            integrationEvent,
            IntegrationEventJsonSerializer.Options);

        var message = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            Subject = typeof(T).Name
        };

        if (integrationEvent is OrderSubmittedIntegrationEvent orderSubmitted)
        {
            message.MessageId =
                $"order-submitted:{orderSubmitted.OrderId}";

            message.CorrelationId =
                orderSubmitted.CorrelationId;
        }

        AddTraceContext(message);

        await _sender.SendMessageAsync(
            message,
            cancellationToken);
    }

    private static void AddTraceContext(
        ServiceBusMessage message)
    {
        var currentActivity =
            Activity.Current;

        if (currentActivity is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(currentActivity.Id))
        {
            return;
        }

        message.ApplicationProperties[TraceParentPropertyName] =
            currentActivity.Id;
    }
}