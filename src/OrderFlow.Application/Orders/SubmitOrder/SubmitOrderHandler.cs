namespace OrderFlow.Application.Orders.SubmitOrder;

public sealed class SubmitOrderHandler : ISubmitOrderHandler
{
    public Task HandleAsync(
        SubmitOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return Task.CompletedTask;
    }
}