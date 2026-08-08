namespace OrderFlow.Application.Orders.Events;

public sealed record OrderSubmittedItem(
    string ProductId,
    int Quantity,
    decimal UnitPrice,
    string Currency);