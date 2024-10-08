using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace BuildingBlocks.SpectreConsole;

public sealed class CustomTypeRegistrar(IHost host) : ITypeRegistrar
{
    public ITypeResolver Build()
    {
        return new TypeResolver(host.Services);
    }

    public void Register(Type service, Type implementation) { }

    public void RegisterInstance(Type service, object implementation) { }

    public void RegisterLazy(Type service, Func<object> func) { }
}
