using OrderFlow.Application.Inventory;

namespace OrderFlow.Infrastructure.Inventory;

public sealed class SimulatedInventoryReservationService
    : IInventoryReservationService
{
    public Task<InventoryReservationResult> ReserveAsync(
        IReadOnlyCollection<InventoryReservationItem> items,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        var unavailableItem = items
            .FirstOrDefault(item =>
                item.ProductId.StartsWith(
                    "OUT-OF-STOCK",
                    StringComparison.OrdinalIgnoreCase));

        if (unavailableItem is not null)
        {
            return Task.FromResult(
                new InventoryReservationResult(
                    Succeeded: false,
                    FailureReason:
                        $"Insufficient inventory for product '{unavailableItem.ProductId}'."));
        }

        return Task.FromResult(
            new InventoryReservationResult(
                Succeeded: true));
    }
}