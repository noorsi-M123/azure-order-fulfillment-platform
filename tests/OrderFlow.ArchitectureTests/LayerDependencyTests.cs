using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace OrderFlow.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(OrderFlow.Domain.AssemblyReference).Assembly,
            typeof(OrderFlow.Application.AssemblyReference).Assembly,
            typeof(OrderFlow.Contracts.AssemblyReference).Assembly,
            typeof(OrderFlow.Infrastructure.AssemblyReference).Assembly)
        .Build();

    private static readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInAssembly(typeof(OrderFlow.Domain.AssemblyReference).Assembly)
            .As("Domain layer");

    private static readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInAssembly(typeof(OrderFlow.Application.AssemblyReference).Assembly)
            .As("Application layer");

    private static readonly IObjectProvider<IType> ContractsLayer =
        Types().That().ResideInAssembly(typeof(OrderFlow.Contracts.AssemblyReference).Assembly)
            .As("Contracts layer");

    private static readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInAssembly(typeof(OrderFlow.Infrastructure.AssemblyReference).Assembly)
            .As("Infrastructure layer");

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        Types()
            .That().Are(DomainLayer)
            .Should()
            .NotDependOnAny(ApplicationLayer)
            .Because("the domain must remain independent from application workflows")
            .Check(Architecture);
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        Types()
            .That().Are(DomainLayer)
            .Should()
            .NotDependOnAny(InfrastructureLayer)
            .Because("the domain must not depend on technical implementations")
            .Check(Architecture);
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        Types()
            .That().Are(ApplicationLayer)
            .Should()
            .NotDependOnAny(InfrastructureLayer)
            .Because("dependency inversion requires infrastructure to implement application ports")
            .Check(Architecture);
    }

    [Fact]
    public void Contracts_Should_Not_Depend_On_Internal_Layers()
    {
        Types()
            .That().Are(ContractsLayer)
            .Should()
            .NotDependOnAny(DomainLayer)
            .AndShould()
            .NotDependOnAny(ApplicationLayer)
            .AndShould()
            .NotDependOnAny(InfrastructureLayer)
            .Because("external contracts must remain independent from internal implementation details")
            .Check(Architecture);
    }
}