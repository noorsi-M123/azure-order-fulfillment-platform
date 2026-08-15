using OrderFlow.Application.Inventory;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Application.Orders.ProcessOrderSubmitted;
using OrderFlow.Application.Orders.Processing;
using Xunit;

namespace OrderFlow.Application.UnitTests.Orders.ProcessOrderSubmitted;

public sealed class ProcessOrderSubmittedHandlerTests
{
    private const string MessageId =
        "order-submitted:ORD-001";

    private const string OrderId =
        "ORD-001";

    private const string CustomerId =
        "CUST-001";

    private const string CorrelationId =
        "corr-test-001";

    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 8, 15, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_ReturnsAlreadyProcessed_WhenMessageWasAlreadyProcessed()
    {
        // Arrange
        var processedMessageStore =
            new FakeProcessedMessageStore
            {
                HasBeenProcessed = true
            };

        var inventoryReservationService =
            new FakeInventoryReservationService();

        var processingResultStore =
            new FakeOrderProcessingResultStore();

        var timeProvider =
            new FixedTimeProvider(FixedUtcNow);

        var sut = new ProcessOrderSubmittedHandler(
            processedMessageStore,
            inventoryReservationService,
            processingResultStore,
            timeProvider);

        var integrationEvent =
            CreateEvent();

        // Act
        var result = await sut.HandleAsync(
            MessageId,
            integrationEvent);

        // Assert
        Assert.Equal(
            ProcessOrderSubmittedResult.AlreadyProcessed,
            result);

        Assert.Equal(
            0,
            inventoryReservationService.ReservationCalls);

        Assert.Null(
            processingResultStore.SavedResult);

        Assert.False(
            processedMessageStore.MarkAsProcessedCalled);
    }

    [Fact]
    public async Task HandleAsync_SavesCompletedResult_WhenInventoryReservationSucceeds()
    {
        // Arrange
        var processedMessageStore =
            new FakeProcessedMessageStore();

        var inventoryReservationService =
            new FakeInventoryReservationService
            {
                ReservationResult =
                    new InventoryReservationResult(
                        Succeeded: true)
            };

        var processingResultStore =
            new FakeOrderProcessingResultStore();

        var timeProvider =
            new FixedTimeProvider(FixedUtcNow);

        var sut = new ProcessOrderSubmittedHandler(
            processedMessageStore,
            inventoryReservationService,
            processingResultStore,
            timeProvider);

        var integrationEvent =
            CreateEvent();

        // Act
        var result = await sut.HandleAsync(
            MessageId,
            integrationEvent);

        // Assert
        Assert.Equal(
            ProcessOrderSubmittedResult.Processed,
            result);

        Assert.NotNull(
            processingResultStore.SavedResult);

        Assert.Equal(
            OrderId,
            processingResultStore.SavedResult.OrderId);

        Assert.Equal(
            OrderProcessingStatus.Completed,
            processingResultStore.SavedResult.Status);

        Assert.Null(
            processingResultStore.SavedResult.FailureReason);

        Assert.Equal(
            FixedUtcNow,
            processingResultStore.SavedResult.ProcessedAtUtc);

        Assert.True(
            processedMessageStore.MarkAsProcessedCalled);
    }

    [Fact]
    public async Task HandleAsync_SavesFailedResult_WhenInventoryReservationFails()
    {
        // Arrange
        const string failureReason =
            "Insufficient inventory.";

        var processedMessageStore =
            new FakeProcessedMessageStore();

        var inventoryReservationService =
            new FakeInventoryReservationService
            {
                ReservationResult =
                    new InventoryReservationResult(
                        Succeeded: false,
                        FailureReason: failureReason)
            };

        var processingResultStore =
            new FakeOrderProcessingResultStore();

        var timeProvider =
            new FixedTimeProvider(FixedUtcNow);

        var sut = new ProcessOrderSubmittedHandler(
            processedMessageStore,
            inventoryReservationService,
            processingResultStore,
            timeProvider);

        var integrationEvent =
            CreateEvent();

        // Act
        var result = await sut.HandleAsync(
            MessageId,
            integrationEvent);

        // Assert
        Assert.Equal(
            ProcessOrderSubmittedResult.Processed,
            result);

        Assert.NotNull(
            processingResultStore.SavedResult);

        Assert.Equal(
            OrderId,
            processingResultStore.SavedResult.OrderId);

        Assert.Equal(
            OrderProcessingStatus.Failed,
            processingResultStore.SavedResult.Status);

        Assert.Equal(
            failureReason,
            processingResultStore.SavedResult.FailureReason);

        Assert.Equal(
            FixedUtcNow,
            processingResultStore.SavedResult.ProcessedAtUtc);

        Assert.True(
            processedMessageStore.MarkAsProcessedCalled);
    }

    private static OrderSubmittedIntegrationEvent CreateEvent()
    {
        return new OrderSubmittedIntegrationEvent(
            OrderId,
            CustomerId,
            new[]
            {
                new OrderSubmittedItem(
                    "PROD-001",
                    2,
                    12.50m,
                    "EUR")
            },
            FixedUtcNow.AddMinutes(-5),
            CorrelationId);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(
            DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    private sealed class FakeProcessedMessageStore
        : IProcessedMessageStore
    {
        public bool HasBeenProcessed { get; init; }

        public bool MarkAsProcessedCalled { get; private set; }

        public Task<bool> HasBeenProcessedAsync(
            string messageId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                HasBeenProcessed);
        }

        public Task MarkAsProcessedAsync(
            string messageId,
            CancellationToken cancellationToken = default)
        {
            MarkAsProcessedCalled = true;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeInventoryReservationService
        : IInventoryReservationService
    {
        public int ReservationCalls { get; private set; }

        public InventoryReservationResult ReservationResult { get; init; } =
            new(Succeeded: true);

        public Task<InventoryReservationResult> ReserveAsync(
            IReadOnlyCollection<InventoryReservationItem> items,
            CancellationToken cancellationToken = default)
        {
            ReservationCalls++;

            return Task.FromResult(
                ReservationResult);
        }
    }

    private sealed class FakeOrderProcessingResultStore
        : IOrderProcessingResultStore
    {
        public OrderProcessingResult? SavedResult { get; private set; }

        public Task SaveAsync(
            OrderProcessingResult result,
            CancellationToken cancellationToken = default)
        {
            SavedResult = result;

            return Task.CompletedTask;
        }
    }
}