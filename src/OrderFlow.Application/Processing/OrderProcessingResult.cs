namespace OrderFlow.Application.Orders.Processing;

public sealed record OrderProcessingResult(
    string OrderId,
    OrderProcessingStatus Status,
    string? FailureReason,
    DateTimeOffset ProcessedAtUtc);