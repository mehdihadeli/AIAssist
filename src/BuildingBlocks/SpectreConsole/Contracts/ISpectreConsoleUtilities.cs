namespace BuildingBlocks.SpectreConsole.Contracts;

public interface ISpectreConsoleUtilities
{
    bool ConfirmationPrompt(string message);
    string UserPrompt(string promptMessage);
    void InformationText(string message);
    void Text(string message);
    void ErrorText(string message);
    void SuccessText(string message);
    void Exception(string errorMessage, Exception ex);
}
