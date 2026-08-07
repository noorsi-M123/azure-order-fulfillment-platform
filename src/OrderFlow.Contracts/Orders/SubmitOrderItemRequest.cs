namespace OrderFlow.Contracts.Orders;

public sealed record SubmitOrderItemRequest(
    string ProductId,
    int Quantity,
    decimal UnitPrice,
    string Currency);