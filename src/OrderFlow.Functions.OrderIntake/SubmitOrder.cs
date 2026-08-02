using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OrderFlow.Functions.OrderIntake;

public sealed class SubmitOrder
{
    private readonly ILogger<SubmitOrder> _logger;

    public SubmitOrder(ILogger<SubmitOrder> logger)
    {
        _logger = logger;
    }

    [Function(nameof(SubmitOrder))]
    public IActionResult Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "orders")]
        HttpRequest request)
    {
        _logger.LogInformation(
            "Order submission request received. TraceIdentifier: {TraceIdentifier}",
            request.HttpContext.TraceIdentifier);

        return new AcceptedResult(
            location: null,
            value: new
            {
                message = "Order submission accepted for processing."
            });
    }
}