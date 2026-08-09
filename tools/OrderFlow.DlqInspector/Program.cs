using Azure.Messaging.ServiceBus;

const string defaultConnectionString =
    "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

const string defaultQueueName = "orders-submitted";

var connectionString =
    Environment.GetEnvironmentVariable("ORDERFLOW_SERVICEBUS_CONNECTION")
    ?? defaultConnectionString;

var queueName =
    Environment.GetEnvironmentVariable("ORDERFLOW_SERVICEBUS_QUEUE")
    ?? defaultQueueName;

Console.WriteLine("OrderFlow DLQ Inspector");
Console.WriteLine("=======================");
Console.WriteLine($"Queue: {queueName}");
Console.WriteLine();

await using var client = new ServiceBusClient(connectionString);

var receiverOptions = new ServiceBusReceiverOptions
{
    SubQueue = SubQueue.DeadLetter,
    ReceiveMode = ServiceBusReceiveMode.PeekLock
};

await using var receiver =
    client.CreateReceiver(queueName, receiverOptions);

var messages = await receiver.PeekMessagesAsync(maxMessages: 20);

if (messages.Count == 0)
{
    Console.WriteLine("No messages found in the dead-letter queue.");
    return;
}

Console.WriteLine($"Found {messages.Count} dead-letter message(s).");
Console.WriteLine();

foreach (var message in messages)
{
    Console.WriteLine("----------------------------------------");
    Console.WriteLine($"MessageId:             {message.MessageId}");
    Console.WriteLine($"SequenceNumber:        {message.SequenceNumber}");
    Console.WriteLine($"DeliveryCount:         {message.DeliveryCount}");
    Console.WriteLine($"EnqueuedTimeUtc:       {message.EnqueuedTime}");
    Console.WriteLine($"DeadLetterReason:      {message.DeadLetterReason}");
    Console.WriteLine($"DeadLetterDescription: {message.DeadLetterErrorDescription}");
}

Console.WriteLine("----------------------------------------");