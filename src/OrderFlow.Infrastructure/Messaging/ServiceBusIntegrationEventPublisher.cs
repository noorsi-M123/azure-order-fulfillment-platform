using Azure.Messaging.ServiceBus;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Infrastructure.Messaging;

public sealed class ServiceBusIntegrationEventPublisher
    : IIntegrationEventPublisher
{
    private const string QueueName = "orders-submitted";

    private readonly ServiceBusSender _sender;

    public ServiceBusIntegrationEventPublisher(
        ServiceBusClient serviceBusClient)
    {
        _sender = serviceBusClient.CreateSender(QueueName);
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
            var messageId = $"order-submitted:{orderSubmitted.OrderId}";

            message.MessageId = messageId;
            message.CorrelationId = messageId;
        }

        await _sender.SendMessageAsync(
            message,
            cancellationToken);
    }
}