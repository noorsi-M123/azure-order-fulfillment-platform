using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Application.Orders.ProcessOrderSubmitted;

namespace OrderFlow.Application.UnitTests.Orders.ProcessOrderSubmitted;

public sealed class ProcessOrderSubmittedHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenMessageWasAlreadyProcessed_ReturnsAlreadyProcessed()
    {
        var store = new FakeProcessedMessageStore
        {
            HasBeenProcessed = true
        };

        var sut = new ProcessOrderSubmittedHandler(store);

        var integrationEvent = CreateEvent();

        var result = await sut.HandleAsync(
            "order-submitted:ORD-001",
            integrationEvent);

        Assert.Equal(
            ProcessOrderSubmittedResult.AlreadyProcessed,
            result);

        Assert.False(store.MarkAsProcessedCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageWasNotProcessed_MarksMessageAsProcessed()
    {
        var store = new FakeProcessedMessageStore
        {
            HasBeenProcessed = false
        };

        var sut = new ProcessOrderSubmittedHandler(store);

        var integrationEvent = CreateEvent();

        var result = await sut.HandleAsync(
            "order-submitted:ORD-001",
            integrationEvent);

        Assert.Equal(
            ProcessOrderSubmittedResult.Processed,
            result);

        Assert.True(store.MarkAsProcessedCalled);
        Assert.Equal(
            "order-submitted:ORD-001",
            store.MarkedMessageId);
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
}