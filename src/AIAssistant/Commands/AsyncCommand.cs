using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssistant.Commands;

public abstract class AsyncCommand<TSettings> : ICommand<TSettings>
    where TSettings : CommandSettings
{
    public ValidationResult Validate(CommandContext context, CommandSettings settings)
    {
        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    Task<int> ICommand.Execute(CommandContext context, CommandSettings settings)
    {
        Debug.Assert(settings is TSettings, "Command settings is of unexpected type.");
        return ExecuteAsync(context, (TSettings)settings);
    }

    /// <inheritdoc/>
    Task<int> ICommand<TSettings>.Execute(CommandContext context, TSettings settings)
    {
        return ExecuteAsync(context, settings);
    }

    ValidationResult ICommand.Validate(CommandContext context, CommandSettings settings)
    {
        return Validate(context, (TSettings)settings);
    }

    public abstract Task<int> ExecuteAsync(CommandContext context, TSettings settings);
}
