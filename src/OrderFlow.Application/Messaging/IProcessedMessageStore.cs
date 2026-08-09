namespace OrderFlow.Application.Messaging;

public interface IProcessedMessageStore
{
    Task<bool> HasBeenProcessedAsync(
        string messageId,
        CancellationToken cancellationToken = default);

    Task MarkAsProcessedAsync(
        string messageId,
        CancellationToken cancellationToken = default);
}