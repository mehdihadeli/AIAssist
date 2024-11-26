using AIAssist.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIAssist.Commands.InternalCommands;

public class SummarizeCommand(ISpectreUtilities spectreUtilities, IOptions<AppOptions> appOptions)
    : IInternalConsoleCommand
{
    public string Name => AIAssistConstants.InternalCommands.Summarize;
    public string Command => $":{Name}";
    public string? ShortCommand => ":s";
    public ConsoleKey? ShortcutKey => ConsoleKey.S;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        return Task.FromResult(true);
    }
}
