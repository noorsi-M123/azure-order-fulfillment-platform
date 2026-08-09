using OrderFlow.Application.Inventory;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Application.Orders.Processing;

namespace OrderFlow.Application.Orders.ProcessOrderSubmitted;

public sealed class ProcessOrderSubmittedHandler
{
    private readonly IProcessedMessageStore _processedMessageStore;
    private readonly IInventoryReservationService _inventoryReservationService;
    private readonly IOrderProcessingResultStore _processingResultStore;

    public ProcessOrderSubmittedHandler(
        IProcessedMessageStore processedMessageStore,
        IInventoryReservationService inventoryReservationService,
        IOrderProcessingResultStore processingResultStore)
    {
        _processedMessageStore = processedMessageStore;
        _inventoryReservationService = inventoryReservationService;
        _processingResultStore = processingResultStore;
    }

    public async Task<ProcessOrderSubmittedResult> HandleAsync(
        string messageId,
        OrderSubmittedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var alreadyProcessed =
            await _processedMessageStore.HasBeenProcessedAsync(
                messageId,
                cancellationToken);

        if (alreadyProcessed)
        {
            return ProcessOrderSubmittedResult.AlreadyProcessed;
        }

        var reservationItems = integrationEvent.Items
            .Select(item => new InventoryReservationItem(
                item.ProductId,
                item.Quantity))
            .ToArray();

        var reservationResult =
            await _inventoryReservationService.ReserveAsync(
                reservationItems,
                cancellationToken);

        var processingResult = new OrderProcessingResult(
            integrationEvent.OrderId,
            reservationResult.Succeeded
                ? OrderProcessingStatus.Completed
                : OrderProcessingStatus.Failed,
            reservationResult.FailureReason,
            DateTimeOffset.UtcNow);

        await _processingResultStore.SaveAsync(
            processingResult,
            cancellationToken);

        await _processedMessageStore.MarkAsProcessedAsync(
            messageId,
            cancellationToken);

        return ProcessOrderSubmittedResult.Processed;
    }
}