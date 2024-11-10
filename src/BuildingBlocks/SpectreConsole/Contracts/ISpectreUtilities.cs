using Spectre.Console;

namespace BuildingBlocks.SpectreConsole.Contracts;

public interface ISpectreUtilities
{
    bool ConfirmationPrompt(string message);
    string? UserPrompt(string? promptMessage = null);
    void InformationText(string message, Justify? justify = null, Overflow? overflow = null);
    void NormalText(string message, Justify? justify = null, Overflow? overflow = null);
    void WarningText(string message, Justify? justify = null, Overflow? overflow = null);
    void ErrorText(string message, Justify? justify = null, Overflow? overflow = null);
    void SuccessText(string message, Justify? justify = null, Overflow? overflow = null);
    void WriteCursor();
    void WriteRule();
    void Exception(string errorMessage, Exception ex);
    void DirectoryTree(string path, int indentLevel);
    public IEnumerable<string> GetArguments(string input);
    bool PressedShortcutKey(
        IList<IInternalConsoleCommand> commands,
        ConsoleModifiers consoleModifierKey,
        out string pressedKey
    );
    void Clear();
}
