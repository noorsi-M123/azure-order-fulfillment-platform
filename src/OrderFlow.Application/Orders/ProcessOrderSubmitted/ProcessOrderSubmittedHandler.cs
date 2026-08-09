using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Application.Orders.ProcessOrderSubmitted;

public sealed class ProcessOrderSubmittedHandler
{
    private readonly IProcessedMessageStore _processedMessageStore;

    public ProcessOrderSubmittedHandler(
        IProcessedMessageStore processedMessageStore)
    {
        _processedMessageStore = processedMessageStore;
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

// Reserve inventory and persist the processing result before marking the message as processed.
        await _processedMessageStore.MarkAsProcessedAsync(
            messageId,
            cancellationToken);

        return ProcessOrderSubmittedResult.Processed;
    }
}