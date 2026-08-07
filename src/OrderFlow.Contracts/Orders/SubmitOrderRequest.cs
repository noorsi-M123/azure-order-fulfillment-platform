namespace OrderFlow.Contracts.Orders;

public sealed record SubmitOrderRequest(
    string OrderId,
    string CustomerId,
    IReadOnlyCollection<SubmitOrderItemRequest> Items);