using Azure.Data.Tables;
using OrderFlow.Application.Orders.Processing;

namespace OrderFlow.Infrastructure.Orders.Processing;

public sealed class TableOrderProcessingResultStore
    : IOrderProcessingResultStore
{
    private const string PartitionKey = "OrderProcessing";

    private readonly TableClient _tableClient;

    public TableOrderProcessingResultStore(
        TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(
            "OrderProcessingResults");
    }

    public async Task SaveAsync(
        OrderProcessingResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var entity = new TableEntity(
            PartitionKey,
            result.OrderId)
        {
            ["Status"] = result.Status.ToString(),
            ["FailureReason"] = result.FailureReason,
            ["ProcessedAtUtc"] = result.ProcessedAtUtc
        };

        await _tableClient.UpsertEntityAsync(
            entity,
            TableUpdateMode.Replace,
            cancellationToken);
    }
}