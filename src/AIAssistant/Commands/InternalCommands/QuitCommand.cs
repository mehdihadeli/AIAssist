using AIAssistant.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIAssistant.Commands.InternalCommands;

public class QuitCommand(ISpectreUtilities spectreUtilities, IOptions<AppOptions> appOptions) : IInternalConsoleCommand
{
    public string Name => AIAssistantConstants.InternalCommands.Quit;
    public string Command => $":{Name}";
    public string? ShortCommand => ":q";
    public ConsoleKey? ShortcutKey => ConsoleKey.C;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        spectreUtilities.ErrorText("Process interrupted. Exiting...");

        // stop running commands
        return Task.FromResult(false);
    }
}
