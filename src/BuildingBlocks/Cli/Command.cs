using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Cli;

public abstract class Command<TOptions, TCommandHandler> : Command
    where TOptions : class, ICommandOptions
    where TCommandHandler : class, ICommandHandler<TOptions>
{
    protected Command(string name, string description)
        : base(name, description)
    {
        Handler = CommandHandler.Create<TOptions, IServiceProvider, CancellationToken>(HandleCommandAsync);
    }

    // https://github.com/dotnet/command-line-api/blob/2.0.0-beta4.22272.1/docs/model-binding.md#binding-parameters-to-a-command-handler
    private static async Task<int> HandleCommandAsync(
        TOptions options,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    )
    {
        // True dependency injection happening here
        var handler = ActivatorUtilities.CreateInstance<TCommandHandler>(serviceProvider);
        return await handler.HandleAsync(options, cancellationToken);
    }
}
