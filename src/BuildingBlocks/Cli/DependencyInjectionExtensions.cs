using System.CommandLine.Builder;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Cli;

// https://anthonysimmon.com/true-dependency-injection-with-system-commandline/
// https://github.com/dotnet/command-line-api/blob/2.0.0-beta4.22272.1/docs/model-binding.md#binding-parameters-to-a-command-handler

internal static class DependencyInjectionExtensions
{
    public static CommandLineBuilder SetupTerminalDependencySupport(this CommandLineBuilder builder, IHost host)
    {
        return builder.AddMiddleware(
            async (context, next) =>
            {
                // https://github.com/dotnet/command-line-api/blob/2.0.0-beta4.22272.1/src/System.CommandLine/Invocation/ServiceProvider.cs
                context.BindingContext.AddService<IServiceProvider>(sp =>
                {
                    // replace binding context inter service provider with our app service provider
                    sp = host.Services;

                    return sp;
                });

                await next(context);
            }
        );
    }
}
