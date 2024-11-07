namespace BuildingBlocks.SpectreConsole.Contracts;

public interface IInternalConsoleCommand
{
    public string Name { get; }
    public string Command { get; }
    public string? ShortCommand { get; }
    public ConsoleKey? ShortcutKey { get; }
}
