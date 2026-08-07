using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Orders;

namespace OrderFlow.Functions.OrderIntake;

public sealed class SubmitOrder
{
    private readonly ILogger<SubmitOrder> _logger;
    private readonly IValidator<SubmitOrderRequest> _validator;

    public SubmitOrder(
        ILogger<SubmitOrder> logger,
        IValidator<SubmitOrderRequest> validator)
    {
        _logger = logger;
        _validator = validator;
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

        var validationResult = await _validator.ValidateAsync(order);

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

        _logger.LogInformation(
            "Order submission received. OrderId: {OrderId}, CustomerId: {CustomerId}, TraceIdentifier: {TraceIdentifier}",
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