using AIAssistant.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIAssistant.Commands.InternalCommands;

public class ClearHistoryCommand(ISpectreUtilities spectreUtilities, IOptions<AppOptions> appOptions)
    : IInternalConsoleCommand
{
    public string Name => AIAssistantConstants.InternalCommands.ClearHistory;
    public string Command => $":{Name}";
    public string? ShortCommand => ":g";
    public ConsoleKey? ShortcutKey => ConsoleKey.G;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        spectreUtilities.InformationText("History cleared.");

        return Task.FromResult(true);
    }
}
