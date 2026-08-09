using OrderFlow.Application.Inventory;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Application.Orders.ProcessOrderSubmitted;
using OrderFlow.Application.Orders.Processing;

namespace OrderFlow.Application.UnitTests.Orders.ProcessOrderSubmitted;

public sealed class ProcessOrderSubmittedHandlerTests
{
    private const string MessageId = "order-submitted:ORD-001";
    private const string OrderId = "ORD-001";
    private const string CustomerId = "CUST-001";
    private const string CorrelationId = "corr-test-001";

    [Fact]
    public async Task HandleAsync_WhenMessageWasAlreadyProcessed_ReturnsAlreadyProcessed()
    {
        var processedMessageStore = new FakeProcessedMessageStore
        {
            HasBeenProcessed = true
        };

        var inventoryService = new FakeInventoryReservationService();
        var processingResultStore = new FakeOrderProcessingResultStore();

        var sut = new ProcessOrderSubmittedHandler(
            processedMessageStore,
            inventoryService,
            processingResultStore);

        var result = await sut.HandleAsync(
            MessageId,
            CreateEvent());

        Assert.Equal(
            ProcessOrderSubmittedResult.AlreadyProcessed,
            result);

        Assert.False(processedMessageStore.MarkAsProcessedCalled);
        Assert.False(inventoryService.ReserveCalled);
        Assert.False(processingResultStore.SaveCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenInventoryReservationSucceeds_PersistsCompletedResultAndMarksMessageAsProcessed()
    {
        var processedMessageStore = new FakeProcessedMessageStore
        {
            HasBeenProcessed = false
        };

        var inventoryService = new FakeInventoryReservationService
        {
            Result = new InventoryReservationResult(
                Succeeded: true)
        };

        var processingResultStore = new FakeOrderProcessingResultStore();

        var sut = new ProcessOrderSubmittedHandler(
            processedMessageStore,
            inventoryService,
            processingResultStore);

        var result = await sut.HandleAsync(
            MessageId,
            CreateEvent());

        Assert.Equal(
            ProcessOrderSubmittedResult.Processed,
            result);

        Assert.True(inventoryService.ReserveCalled);
        Assert.True(processingResultStore.SaveCalled);

        Assert.NotNull(processingResultStore.SavedResult);

        Assert.Equal(
            OrderId,
            processingResultStore.SavedResult.OrderId);

        Assert.Equal(
            OrderProcessingStatus.Completed,
            processingResultStore.SavedResult.Status);

        Assert.Null(
            processingResultStore.SavedResult.FailureReason);

        Assert.True(processedMessageStore.MarkAsProcessedCalled);

        Assert.Equal(
            MessageId,
            processedMessageStore.MarkedMessageId);
    }

    [Fact]
    public async Task HandleAsync_WhenInventoryReservationFails_PersistsFailedResultAndMarksMessageAsProcessed()
    {
        const string failureReason = "Insufficient inventory.";

        var processedMessageStore = new FakeProcessedMessageStore
        {
            HasBeenProcessed = false
        };

        var inventoryService = new FakeInventoryReservationService
        {
            Result = new InventoryReservationResult(
                Succeeded: false,
                FailureReason: failureReason)
        };

        var processingResultStore = new FakeOrderProcessingResultStore();

        var sut = new ProcessOrderSubmittedHandler(
            processedMessageStore,
            inventoryService,
            processingResultStore);

        var result = await sut.HandleAsync(
            MessageId,
            CreateEvent());

        Assert.Equal(
            ProcessOrderSubmittedResult.Processed,
            result);

        Assert.True(inventoryService.ReserveCalled);
        Assert.True(processingResultStore.SaveCalled);

        Assert.NotNull(processingResultStore.SavedResult);

        Assert.Equal(
            OrderId,
            processingResultStore.SavedResult.OrderId);

        Assert.Equal(
            OrderProcessingStatus.Failed,
            processingResultStore.SavedResult.Status);

        Assert.Equal(
            failureReason,
            processingResultStore.SavedResult.FailureReason);

        Assert.True(processedMessageStore.MarkAsProcessedCalled);

        Assert.Equal(
            MessageId,
            processedMessageStore.MarkedMessageId);
    }

    private static OrderSubmittedIntegrationEvent CreateEvent()
    {
        return new OrderSubmittedIntegrationEvent(
            OrderId,
            CustomerId,
            [
                new OrderSubmittedItem(
                    "PROD-001",
                    1,
                    10m,
                    "EUR")
            ],
            DateTimeOffset.UtcNow,
            CorrelationId);
    }

    private sealed class FakeProcessedMessageStore
        : IProcessedMessageStore
    {
        public bool HasBeenProcessed { get; init; }

        public bool MarkAsProcessedCalled { get; private set; }

        public string? MarkedMessageId { get; private set; }

        public Task<bool> HasBeenProcessedAsync(
            string messageId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HasBeenProcessed);
        }

        public Task MarkAsProcessedAsync(
            string messageId,
            CancellationToken cancellationToken = default)
        {
            MarkAsProcessedCalled = true;
            MarkedMessageId = messageId;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeInventoryReservationService
        : IInventoryReservationService
    {
        public InventoryReservationResult Result { get; init; } =
            new(Succeeded: true);

        public bool ReserveCalled { get; private set; }

        public Task<InventoryReservationResult> ReserveAsync(
            IReadOnlyCollection<InventoryReservationItem> items,
            CancellationToken cancellationToken = default)
        {
            ReserveCalled = true;

            return Task.FromResult(Result);
        }
    }

    private sealed class FakeOrderProcessingResultStore
        : IOrderProcessingResultStore
    {
        public bool SaveCalled { get; private set; }

        public OrderProcessingResult? SavedResult { get; private set; }

        public Task SaveAsync(
            OrderProcessingResult result,
            CancellationToken cancellationToken = default)
        {
            SaveCalled = true;
            SavedResult = result;

            return Task.CompletedTask;
        }
    }
}