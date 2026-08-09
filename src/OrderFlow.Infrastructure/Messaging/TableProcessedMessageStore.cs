using Azure;
using Azure.Data.Tables;
using OrderFlow.Application.Messaging;

namespace OrderFlow.Infrastructure.Messaging;

public sealed class TableProcessedMessageStore
    : IProcessedMessageStore
{
    private const string PartitionKey = "OrderSubmitted";

    private readonly TableClient _tableClient;

    public TableProcessedMessageStore(
        TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(
            "ProcessedMessages");
    }

    public async Task<bool> HasBeenProcessedAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        var response = await _tableClient.GetEntityIfExistsAsync<TableEntity>(
            PartitionKey,
            messageId,
            cancellationToken: cancellationToken);

        return response.HasValue;
    }

    public async Task MarkAsProcessedAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        var entity = new TableEntity(
            PartitionKey,
            messageId)
        {
            ["ProcessedAtUtc"] = DateTimeOffset.UtcNow
        };

        await _tableClient.UpsertEntityAsync(
            entity,
            TableUpdateMode.Replace,
            cancellationToken);
    }
}