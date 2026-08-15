using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Application.Orders.SubmitOrder;
using Xunit;

namespace OrderFlow.Application.UnitTests.Orders.SubmitOrder;

public sealed class SubmitOrderHandlerTests
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 8, 15, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_PublishesOrderSubmittedEvent_WithExpectedData()
    {
        // Arrange
        var publisher =
            new CapturingIntegrationEventPublisher();

        var timeProvider =
            new FixedTimeProvider(FixedUtcNow);

        var sut = new SubmitOrderHandler(
            publisher,
            timeProvider);

        var command =
            new SubmitOrderCommand(
                "ORD-001",
                "CUST-001",
                new[]
                {
                    new SubmitOrderItemCommand(
                        "PROD-001",
                        2,
                        12.50m,
                        "EUR")
                },
                "corr-test-001");

        // Act
        await sut.HandleAsync(command);

        // Assert
        var publishedEvent =
            Assert.IsType<OrderSubmittedIntegrationEvent>(
                publisher.PublishedEvent);

        Assert.Equal(
            "ORD-001",
            publishedEvent.OrderId);

        Assert.Equal(
            "CUST-001",
            publishedEvent.CustomerId);

        Assert.Equal(
            "corr-test-001",
            publishedEvent.CorrelationId);

        Assert.Equal(
            FixedUtcNow,
            publishedEvent.OccurredAtUtc);

        var item =
            Assert.Single(publishedEvent.Items);

        Assert.Equal(
            "PROD-001",
            item.ProductId);

        Assert.Equal(
            2,
            item.Quantity);

        Assert.Equal(
            12.50m,
            item.UnitPrice);

        Assert.Equal(
            "EUR",
            item.Currency);
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

    private sealed class CapturingIntegrationEventPublisher
        : IIntegrationEventPublisher
    {
        public object? PublishedEvent { get; private set; }

        public Task PublishAsync<T>(
            T integrationEvent,
            CancellationToken cancellationToken = default)
            where T : class
        {
            PublishedEvent = integrationEvent;

            return Task.CompletedTask;
        }
    }
}