using BuildingBlocks.SpectreConsole.Contracts;

namespace BuildingBlocks.SpectreConsole;

public record InternalConsoleCommand(string Name, string? ShortCommand, ConsoleKey? ShortcutKey = null)
    : IInternalConsoleCommand
{
    public string Command => $":{Name}";
}
