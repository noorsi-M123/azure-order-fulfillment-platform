using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Application.Orders.ProcessOrderSubmitted;
using OrderFlow.Application.Orders.Processing;
using OrderFlow.Infrastructure.Inventory;
using Xunit;

namespace OrderFlow.IntegrationTests;

public sealed class OrderProcessingIntegrationTests
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 8, 15, 11, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ProcessOrderSubmitted_CompletesOrder_WhenInventoryIsAvailable()
    {
        // Arrange
        var processedMessageStore =
            new InMemoryProcessedMessageStore();

        var processingResultStore =
            new InMemoryOrderProcessingResultStore();

        var inventoryReservationService =
            new SimulatedInventoryReservationService();

        var timeProvider =
            new FixedTimeProvider(FixedUtcNow);

        var sut =
            new ProcessOrderSubmittedHandler(
                processedMessageStore,
                inventoryReservationService,
                processingResultStore,
                timeProvider);

        const string messageId =
            "order-submitted:ORD-INT-001";

        var integrationEvent =
            CreateEvent(
                orderId: "ORD-INT-001",
                productId: "PROD-001");

        // Act
        var result =
            await sut.HandleAsync(
                messageId,
                integrationEvent);

        // Assert
        Assert.Equal(
            ProcessOrderSubmittedResult.Processed,
            result);

        Assert.NotNull(
            processingResultStore.SavedResult);

        Assert.Equal(
            "ORD-INT-001",
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
            await processedMessageStore.HasBeenProcessedAsync(
                messageId));
    }

    [Fact]
    public async Task ProcessOrderSubmitted_FailsOrder_WhenInventoryIsUnavailable()
    {
        // Arrange
        var processedMessageStore =
            new InMemoryProcessedMessageStore();

        var processingResultStore =
            new InMemoryOrderProcessingResultStore();

        var inventoryReservationService =
            new SimulatedInventoryReservationService();

        var timeProvider =
            new FixedTimeProvider(FixedUtcNow);

        var sut =
            new ProcessOrderSubmittedHandler(
                processedMessageStore,
                inventoryReservationService,
                processingResultStore,
                timeProvider);

        const string messageId =
            "order-submitted:ORD-INT-002";

        var integrationEvent =
            CreateEvent(
                orderId: "ORD-INT-002",
                productId: "OUT-OF-STOCK-001");

        // Act
        var result =
            await sut.HandleAsync(
                messageId,
                integrationEvent);

        // Assert
        Assert.Equal(
            ProcessOrderSubmittedResult.Processed,
            result);

        Assert.NotNull(
            processingResultStore.SavedResult);

        Assert.Equal(
            "ORD-INT-002",
            processingResultStore.SavedResult.OrderId);

        Assert.Equal(
            OrderProcessingStatus.Failed,
            processingResultStore.SavedResult.Status);

        Assert.Contains(
            "Insufficient inventory",
            processingResultStore.SavedResult.FailureReason);

        Assert.Equal(
            FixedUtcNow,
            processingResultStore.SavedResult.ProcessedAtUtc);

        Assert.True(
            await processedMessageStore.HasBeenProcessedAsync(
                messageId));
    }

    private static OrderSubmittedIntegrationEvent CreateEvent(
        string orderId,
        string productId)
    {
        return new OrderSubmittedIntegrationEvent(
            orderId,
            "CUST-INT-001",
            new[]
            {
                new OrderSubmittedItem(
                    productId,
                    1,
                    25.00m,
                    "EUR")
            },
            FixedUtcNow.AddMinutes(-1),
            "corr-integration-001");
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

    private sealed class InMemoryProcessedMessageStore
        : IProcessedMessageStore
    {
        private readonly HashSet<string> _messageIds =
            new(StringComparer.Ordinal);

        public Task<bool> HasBeenProcessedAsync(
            string messageId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _messageIds.Contains(messageId));
        }

        public Task MarkAsProcessedAsync(
            string messageId,
            CancellationToken cancellationToken = default)
        {
            _messageIds.Add(messageId);

            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryOrderProcessingResultStore
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