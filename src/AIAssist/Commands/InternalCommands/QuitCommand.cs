using AIAssist.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIAssist.Commands.InternalCommands;

public class QuitCommand(ISpectreUtilities spectreUtilities, IOptions<AppOptions> appOptions) : IInternalConsoleCommand
{
    public string Name => AIAssistConstants.InternalCommands.Quit;
    public string Command => $":{Name}";
    public string? ShortCommand => ":q";
    public ConsoleKey? ShortcutKey => ConsoleKey.C;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        spectreUtilities.ErrorTextLine("Process interrupted. Exiting...");

        // stop running commands
        return Task.FromResult(false);
    }
}
