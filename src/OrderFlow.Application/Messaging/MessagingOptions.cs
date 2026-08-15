namespace OrderFlow.Application.Messaging;

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    public string OrdersSubmittedQueueName { get; init; } =
        string.Empty;
}