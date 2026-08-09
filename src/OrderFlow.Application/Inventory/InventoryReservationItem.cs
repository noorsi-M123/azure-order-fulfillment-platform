namespace OrderFlow.Application.Inventory;

public sealed record InventoryReservationItem(
    string ProductId,
    int Quantity);