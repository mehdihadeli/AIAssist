using AIAssist.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIAssist.Commands.InternalCommands;

public class ClearHistoryCommand(ISpectreUtilities spectreUtilities, IOptions<AppOptions> appOptions)
    : IInternalConsoleCommand
{
    public string Name => AIAssistConstants.InternalCommands.ClearHistory;
    public string Command => $":{Name}";
    public string? ShortCommand => ":g";
    public ConsoleKey? ShortcutKey => ConsoleKey.G;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        spectreUtilities.InformationTextLine("History cleared.");

        return Task.FromResult(true);
    }
}
