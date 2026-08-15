using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Application.Orders.SubmitOrder;

public sealed class SubmitOrderHandler : ISubmitOrderHandler
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly TimeProvider _timeProvider;

    public SubmitOrderHandler(
        IIntegrationEventPublisher integrationEventPublisher,
        TimeProvider timeProvider)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(
        SubmitOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var integrationEvent =
            new OrderSubmittedIntegrationEvent(
                command.OrderId,
                command.CustomerId,
                command.Items
                    .Select(item =>
                        new OrderSubmittedItem(
                            item.ProductId,
                            item.Quantity,
                            item.UnitPrice,
                            item.Currency))
                    .ToArray(),
                _timeProvider.GetUtcNow(),
                command.CorrelationId);

        await _integrationEventPublisher.PublishAsync(
            integrationEvent,
            cancellationToken);
    }
}