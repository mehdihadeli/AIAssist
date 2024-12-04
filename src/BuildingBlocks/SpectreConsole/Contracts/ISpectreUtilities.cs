using BuildingBlocks.SpectreConsole.StyleElements;
using Spectre.Console;

namespace BuildingBlocks.SpectreConsole.Contracts;

public interface ISpectreUtilities
{
    ColorTheme Theme { get; }

    bool ConfirmationPrompt(string message);
    string? UserPrompt(string? promptMessage = null);
    void InformationTextLine(
        string message,
        Justify? justify = null,
        Overflow? overflow = null,
        Decoration? decoration = null
    );
    void InformationText(
        string message,
        Justify? justify = null,
        Overflow? overflow = null,
        Decoration? decoration = null
    );
    public void SummaryTextLine(
        string message,
        Justify? justify = null,
        Overflow? overflow = null,
        Decoration? decoration = null
    );
    public void SummaryText(
        string message,
        Justify? justify = null,
        Overflow? overflow = null,
        Decoration? decoration = null
    );
    public void HighlightTextLine(
        string message,
        Justify? justify = null,
        Overflow? overflow = null,
        Decoration? decoration = null
    );
    public void HighlightText(
        string message,
        Justify? justify = null,
        Overflow? overflow = null,
        Decoration? decoration = null
    );
    void NormalTextLine(
        string message,
        Justify? justify = null,
        Overflow? overflow = null,
        Decoration? decoration = null
    );
    void NormalText(string message, Justify? justify = null, Overflow? overflow = null, Decoration? decoration = null);
    void WarningTextLine(
        string message,
        Justify? justify = null,
        Overflow? overflow = null,
        Decoration? decoration = null
    );
    void WarningText(string message, Justify? justify = null, Overflow? overflow = null, Decoration? decoration = null);
    void ErrorTextLine(
        string message,
        Justify? justify = null,
        Overflow? overflow = null,
        Decoration? decoration = null
    );
    void SuccessTextLine(
        string message,
        Justify? justify = null,
        Overflow? overflow = null,
        Decoration? decoration = null
    );
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
    public Style CreateStyle(StyleBase styleBase);
    public string CreateStringStyle(StyleBase styleBase);
}
