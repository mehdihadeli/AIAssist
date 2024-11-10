using AIAssistant.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIAssistant.Commands.InternalCommands;

public class ClearCommand(ISpectreUtilities spectreUtilities, IOptions<AppOptions> appOptions) : IInternalConsoleCommand
{
    public string Name => AIAssistantConstants.InternalCommands.Clear;
    public string Command => $":{Name}";
    public string? ShortCommand => ":c";
    public ConsoleKey? ShortcutKey => ConsoleKey.F;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        spectreUtilities.Clear();

        return Task.FromResult(true);
    }
}
