namespace OrderFlow.Application.Orders.SubmitOrder;

public interface ISubmitOrderHandler
{
    Task HandleAsync(
        SubmitOrderCommand command,
        CancellationToken cancellationToken = default);
}