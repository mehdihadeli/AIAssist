using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AIAssistant.Commands.InternalCommands;

public class HelpCommand(string HelpText, ISpectreUtilities spectreUtilities) : IInternalConsoleCommand
{
    public string Name => AIAssistantConstants.InternalCommands.Help;
    public string Command => $":{Name}";
    public string? ShortCommand => ":h";
    public ConsoleKey? ShortcutKey => ConsoleKey.H;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        return Task.FromResult(true);
    }
}
