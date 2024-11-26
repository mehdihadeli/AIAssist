using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AIAssist.Commands.InternalCommands;

public class HelpCommand(string HelpText, ISpectreUtilities spectreUtilities) : IInternalConsoleCommand
{
    public string Name => AIAssistConstants.InternalCommands.Help;
    public string Command => $":{Name}";
    public string? ShortCommand => ":h";
    public ConsoleKey? ShortcutKey => ConsoleKey.H;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        return Task.FromResult(true);
    }
}
