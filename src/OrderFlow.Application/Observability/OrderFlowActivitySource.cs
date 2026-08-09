using System.Diagnostics;

namespace OrderFlow.Application.Observability;

public static class OrderFlowActivitySource
{
    public const string Name = "OrderFlow";

    public static ActivitySource Instance { get; } =
        new(Name);
}