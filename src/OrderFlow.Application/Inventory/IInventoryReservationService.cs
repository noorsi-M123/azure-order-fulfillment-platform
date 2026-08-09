namespace OrderFlow.Application.Inventory;

public interface IInventoryReservationService
{
    Task<InventoryReservationResult> ReserveAsync(
        IReadOnlyCollection<InventoryReservationItem> items,
        CancellationToken cancellationToken = default);
}