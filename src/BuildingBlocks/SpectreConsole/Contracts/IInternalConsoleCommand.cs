using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.SpectreConsole.Contracts;

public interface IInternalConsoleCommand
{
    string Name { get; }
    string Command { get; }
    string? ShortCommand { get; }
    ConsoleKey? ShortcutKey { get; }
    bool IsDefaultCommand { get; }
    Task<bool> ExecuteAsync(IServiceScope scope, string? input);
}
