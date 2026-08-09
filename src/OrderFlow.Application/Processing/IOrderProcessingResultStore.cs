namespace OrderFlow.Application.Orders.Processing;

public interface IOrderProcessingResultStore
{
    Task SaveAsync(
        OrderProcessingResult result,
        CancellationToken cancellationToken = default);
}