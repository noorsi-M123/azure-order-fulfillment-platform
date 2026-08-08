using System.Text.Json;

namespace OrderFlow.Infrastructure.Messaging;

public static class IntegrationEventJsonSerializer
{
    public static JsonSerializerOptions Options { get; } =
        new(JsonSerializerDefaults.Web);
}