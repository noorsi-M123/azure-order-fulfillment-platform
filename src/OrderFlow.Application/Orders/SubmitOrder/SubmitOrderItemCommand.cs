namespace OrderFlow.Application.Orders.SubmitOrder;

public sealed record SubmitOrderItemCommand(
    string ProductId,
    int Quantity,
    decimal UnitPrice,
    string Currency);