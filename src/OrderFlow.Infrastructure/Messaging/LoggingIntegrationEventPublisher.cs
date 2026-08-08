using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Messaging;

namespace OrderFlow.Infrastructure.Messaging;

public sealed class LoggingIntegrationEventPublisher
    : IIntegrationEventPublisher
{
    private readonly ILogger<LoggingIntegrationEventPublisher> _logger;

    public LoggingIntegrationEventPublisher(
        ILogger<LoggingIntegrationEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(
        T integrationEvent,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var payload = JsonSerializer.Serialize(integrationEvent);

        _logger.LogInformation(
            "Integration event published locally. EventType: {EventType}, Payload: {Payload}",
            typeof(T).Name,
            payload);

        return Task.CompletedTask;
    }
}