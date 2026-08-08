namespace OrderFlow.Application.Orders.Events;

public sealed record OrderSubmittedIntegrationEvent(
    string OrderId,
    string CustomerId,
    IReadOnlyCollection<OrderSubmittedItem> Items,
    DateTimeOffset OccurredAtUtc);