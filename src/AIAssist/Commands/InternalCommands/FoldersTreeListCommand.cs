using AIAssist.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIAssist.Commands.InternalCommands;

public class FoldersTreeListCommand(ISpectreUtilities spectreUtilities, IOptions<AppOptions> appOptions)
    : IInternalConsoleCommand
{
    public string Name => AIAssistConstants.InternalCommands.TreeList;
    public string Command => $":{Name}";
    public string? ShortCommand => ":l";
    public ConsoleKey? ShortcutKey => ConsoleKey.L;
    public bool IsDefaultCommand => false;

    public Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        spectreUtilities.DirectoryTree(appOptions.Value.ContextWorkingDirectory, 0);

        return Task.FromResult(true);
    }
}
