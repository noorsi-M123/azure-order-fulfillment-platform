using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.Application.Orders.SubmitOrder;
using OrderFlow.Contracts.Orders;
using OrderFlow.Functions.OrderIntake;
using Xunit;

namespace OrderFlow.Functions.OrderIntake.UnitTests;

public sealed class SubmitOrderTests
{
    [Fact]
    public async Task Run_UsesValidCorrelationIdFromHeader()
    {
        // Arrange
        var handler = new CapturingSubmitOrderHandler();
        var sut = CreateSut(handler);

        var context = CreateHttpContext(
            correlationId: "corr-valid-001");

        // Act
        var result = await sut.Run(context.Request);

        // Assert
        Assert.IsType<AcceptedResult>(result);

        Assert.Equal(
            "corr-valid-001",
            handler.LastCommand?.CorrelationId);
    }

    [Fact]
    public async Task Run_FallsBackToTraceIdentifier_WhenCorrelationIdIsTooLong()
    {
        // Arrange
        var handler = new CapturingSubmitOrderHandler();
        var sut = CreateSut(handler);

        var context = CreateHttpContext(
            correlationId: new string('a', 101));

        context.TraceIdentifier = "server-trace-001";

        // Act
        var result = await sut.Run(context.Request);

        // Assert
        Assert.IsType<AcceptedResult>(result);

        Assert.Equal(
            "server-trace-001",
            handler.LastCommand?.CorrelationId);
    }

    [Fact]
    public async Task Run_FallsBackToTraceIdentifier_WhenCorrelationIdContainsUnsafeCharacters()
    {
        // Arrange
        var handler = new CapturingSubmitOrderHandler();
        var sut = CreateSut(handler);

        var context = CreateHttpContext(
            correlationId: "corr-001<script>");

        context.TraceIdentifier = "server-trace-002";

        // Act
        var result = await sut.Run(context.Request);

        // Assert
        Assert.IsType<AcceptedResult>(result);

        Assert.Equal(
            "server-trace-002",
            handler.LastCommand?.CorrelationId);
    }

    [Fact]
    public async Task Run_FallsBackToTraceIdentifier_WhenCorrelationIdHeaderIsMissing()
    {
        // Arrange
        var handler = new CapturingSubmitOrderHandler();
        var sut = CreateSut(handler);

        var context = CreateHttpContext();

        context.TraceIdentifier = "server-trace-003";

        // Act
        var result = await sut.Run(context.Request);

        // Assert
        Assert.IsType<AcceptedResult>(result);

        Assert.Equal(
            "server-trace-003",
            handler.LastCommand?.CorrelationId);
    }

    private static SubmitOrder CreateSut(
        CapturingSubmitOrderHandler handler)
    {
        var validator =
            new InlineValidator<SubmitOrderRequest>();

        return new SubmitOrder(
            NullLogger<SubmitOrder>.Instance,
            validator,
            handler);
    }

    private static DefaultHttpContext CreateHttpContext(
        string? correlationId = null)
    {
        var context = new DefaultHttpContext();

        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/json";

        if (correlationId is not null)
        {
            context.Request.Headers["X-Correlation-ID"] =
                correlationId;
        }

        const string json =
            """
            {
              "orderId": "ORD-TEST-001",
              "customerId": "CUST-TEST-001",
              "items": [
                {
                  "productId": "PROD-TEST-001",
                  "quantity": 1,
                  "unitPrice": 10.00,
                  "currency": "EUR"
                }
              ]
            }
            """;

        context.Request.Body =
            new MemoryStream(
                Encoding.UTF8.GetBytes(json));

        return context;
    }

    private sealed class CapturingSubmitOrderHandler
        : ISubmitOrderHandler
    {
        public SubmitOrderCommand? LastCommand { get; private set; }

        public Task HandleAsync(
            SubmitOrderCommand command,
            CancellationToken cancellationToken = default)
        {
            LastCommand = command;

            return Task.CompletedTask;
        }
    }
}