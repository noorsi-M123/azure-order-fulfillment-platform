using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Observability;
using OrderFlow.Application.Orders.SubmitOrder;
using OrderFlow.Contracts.Orders;

namespace OrderFlow.Functions.OrderIntake;

public sealed class SubmitOrder
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

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
        var correlationId = GetCorrelationId(request);

        using var activity =
            OrderFlowActivitySource.Instance.StartActivity(
                "SubmitOrder");

        activity?.SetTag(
            "orderflow.correlation_id",
            correlationId);

        activity?.SetTag(
            "http.request.method",
            request.Method);

        using var requestLoggingScope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });

        var order =
            await request.ReadFromJsonAsync<SubmitOrderRequest>();

        if (order is null)
        {
            _logger.LogWarning(
                "Order submission rejected because the request body is missing.");

            return new BadRequestObjectResult(new
            {
                message = "Request body is required.",
                correlationId
            });
        }

        activity?.SetTag(
            "orderflow.order_id",
            order.OrderId);

        activity?.SetTag(
            "orderflow.customer_id",
            order.CustomerId);

        using var orderLoggingScope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["OrderId"] = order.OrderId,
                ["CustomerId"] = order.CustomerId
            });

        var validationResult =
            await _validator.ValidateAsync(
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

            _logger.LogWarning(
                "Order submission rejected because request validation failed.");

            return new BadRequestObjectResult(new
            {
                message = "Request validation failed.",
                correlationId,
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
                .ToArray(),
            correlationId);

        await _handler.HandleAsync(
            command,
            request.HttpContext.RequestAborted);

        _logger.LogInformation(
            "Order submission accepted for processing.");

        return new AcceptedResult(
            location: null,
            value: new
            {
                message = "Order submission accepted for processing.",
                orderId = order.OrderId,
                correlationId
            });
    }

    private static string GetCorrelationId(
        HttpRequest request)
    {
        if (request.Headers.TryGetValue(
                CorrelationIdHeaderName,
                out var correlationIdHeader))
        {
            var correlationId =
                correlationIdHeader.ToString();

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId;
            }
        }

        return request.HttpContext.TraceIdentifier;
    }
}