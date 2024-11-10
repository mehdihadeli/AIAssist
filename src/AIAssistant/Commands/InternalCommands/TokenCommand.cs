using AIAssistant.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIAssistant.Commands.InternalCommands;

public class TokenCommand(ISpectreUtilities spectreUtilities, IOptions<AppOptions> appOptions) : IInternalConsoleCommand
{
    public string Name => AIAssistantConstants.InternalCommands.Tokens;
    public string Command => $":{Name}";
    public string? ShortCommand => ":t";
    public ConsoleKey? ShortcutKey => ConsoleKey.T;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        return Task.FromResult(true);
    }
}
