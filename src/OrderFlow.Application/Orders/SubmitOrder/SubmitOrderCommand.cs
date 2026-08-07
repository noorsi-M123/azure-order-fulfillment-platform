namespace OrderFlow.Application.Orders.SubmitOrder;

public sealed record SubmitOrderCommand(
    string OrderId,
    string CustomerId,
    IReadOnlyCollection<SubmitOrderItemCommand> Items);