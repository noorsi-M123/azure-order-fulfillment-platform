using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Application.Orders.SubmitOrder;

public sealed class SubmitOrderHandler : ISubmitOrderHandler
{
    private readonly IIntegrationEventPublisher _eventPublisher;

    public SubmitOrderHandler(
        IIntegrationEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task HandleAsync(
        SubmitOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var integrationEvent = new OrderSubmittedIntegrationEvent(
            command.OrderId,
            command.CustomerId,
            command.Items
                .Select(item => new OrderSubmittedItem(
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice,
                    item.Currency))
                .ToArray(),
            DateTimeOffset.UtcNow);

        await _eventPublisher.PublishAsync(
            integrationEvent,
            cancellationToken);
    }
}