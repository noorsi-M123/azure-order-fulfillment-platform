using OrderFlow.Application.Inventory;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Application.Orders.ProcessOrderSubmitted;
using OrderFlow.Application.Orders.Processing;

namespace OrderFlow.Application.UnitTests.Orders.ProcessOrderSubmitted;

public sealed class ProcessOrderSubmittedHandlerTests
{
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

        var integrationEvent = CreateEvent();

        var result = await sut.HandleAsync(
            "order-submitted:ORD-001",
            integrationEvent);

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

        var integrationEvent = CreateEvent();

        var result = await sut.HandleAsync(
            "order-submitted:ORD-001",
            integrationEvent);

        Assert.Equal(
            ProcessOrderSubmittedResult.Processed,
            result);

        Assert.True(inventoryService.ReserveCalled);
        Assert.True(processingResultStore.SaveCalled);

        Assert.NotNull(processingResultStore.SavedResult);
        Assert.Equal(
            OrderProcessingStatus.Completed,
            processingResultStore.SavedResult.Status);

        Assert.True(processedMessageStore.MarkAsProcessedCalled);
        Assert.Equal(
            "order-submitted:ORD-001",
            processedMessageStore.MarkedMessageId);
    }

    [Fact]
    public async Task HandleAsync_WhenInventoryReservationFails_PersistsFailedResultAndMarksMessageAsProcessed()
    {
        var processedMessageStore = new FakeProcessedMessageStore
        {
            HasBeenProcessed = false
        };

        var inventoryService = new FakeInventoryReservationService
        {
            Result = new InventoryReservationResult(
                Succeeded: false,
                FailureReason: "Insufficient inventory.")
        };

        var processingResultStore = new FakeOrderProcessingResultStore();

        var sut = new ProcessOrderSubmittedHandler(
            processedMessageStore,
            inventoryService,
            processingResultStore);

        var result = await sut.HandleAsync(
            "order-submitted:ORD-001",
            CreateEvent());

        Assert.Equal(
            ProcessOrderSubmittedResult.Processed,
            result);

        Assert.NotNull(processingResultStore.SavedResult);

        Assert.Equal(
            OrderProcessingStatus.Failed,
            processingResultStore.SavedResult.Status);

        Assert.Equal(
            "Insufficient inventory.",
            processingResultStore.SavedResult.FailureReason);

        Assert.True(processedMessageStore.MarkAsProcessedCalled);
    }

    private static OrderSubmittedIntegrationEvent CreateEvent()
    {
        return new OrderSubmittedIntegrationEvent(
            "ORD-001",
            "CUST-001",
            [
                new OrderSubmittedItem(
                    "PROD-001",
                    1,
                    10m,
                    "EUR")
            ],
            DateTimeOffset.UtcNow);
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