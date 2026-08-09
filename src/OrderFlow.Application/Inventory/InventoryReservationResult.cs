namespace OrderFlow.Application.Inventory;

public sealed record InventoryReservationResult(
    bool Succeeded,
    string? FailureReason = null);