using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Orders.SubmitOrder;
using OrderFlow.Contracts.Orders;

namespace OrderFlow.Functions.OrderIntake;

public sealed class SubmitOrder
{
    private readonly ILogger<SubmitOrder> _logger;
    private readonly IValidator<SubmitOrderRequest> _validator;
    private readonly ISubmitOrderHandler _handler;

    public SubmitOrder(
        ILogger<SubmitOrder> logger,
        IValidator<SubmitOrderRequest> validator,
        ISubmitOrderHandler handler)
    {
        _logger = logger;
        _validator = validator;
        _handler = handler;
    }

    [Function(nameof(SubmitOrder))]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "orders")]
        HttpRequest request)
    {
        var order = await request.ReadFromJsonAsync<SubmitOrderRequest>();

        if (order is null)
        {
            return new BadRequestObjectResult(new
            {
                message = "Request body is required."
            });
        }

        var validationResult = await _validator.ValidateAsync(
            order,
            request.HttpContext.RequestAborted);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .Distinct()
                        .ToArray());

            return new BadRequestObjectResult(new
            {
                message = "Request validation failed.",
                errors
            });
        }

        var command = new SubmitOrderCommand(
            order.OrderId,
            order.CustomerId,
            order.Items
                .Select(item => new SubmitOrderItemCommand(
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice,
                    item.Currency))
                .ToArray());

        await _handler.HandleAsync(
            command,
            request.HttpContext.RequestAborted);

        _logger.LogInformation(
            "Order submission accepted. OrderId: {OrderId}, CustomerId: {CustomerId}, TraceIdentifier: {TraceIdentifier}",
            order.OrderId,
            order.CustomerId,
            request.HttpContext.TraceIdentifier);

        return new AcceptedResult(
            location: null,
            value: new
            {
                message = "Order submission accepted for processing.",
                orderId = order.OrderId
            });
    }
}