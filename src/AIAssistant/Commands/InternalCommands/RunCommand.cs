using AIAssistant.Chat.Models;
using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Models.Options;
using AIAssistant.Prompts;
using BuildingBlocks.SpectreConsole;
using BuildingBlocks.SpectreConsole.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace AIAssistant.Commands.InternalCommands;

public class RunCommand(
    ISpectreUtilities spectreUtilities,
    IAnsiConsole console,
    IChatSessionManager chatSessionManager,
    IPromptManager promptManager,
    AppOptions appOptions
) : IInternalConsoleCommand
{
    public string Name => AIAssistantConstants.InternalCommands.Run;
    public string Command => $":{Name}";
    public string? ShortCommand => ":r";
    public ConsoleKey? ShortcutKey => ConsoleKey.R;
    public bool IsDefaultCommand => true;

    public async Task<bool> ExecuteAsync(IServiceScope scope, string? input)
    {
        var chatSession = chatSessionManager.GetCurrentActiveSession();
        var codeAssistantManager = scope.ServiceProvider.GetRequiredService<ICodeAssistantManager>();

        var responseStreams = codeAssistantManager.QueryAsync(input);
        var streamPrinter = new StreamPrinter(console, useMarkdown: true);
        var responseContent = await streamPrinter.PrintAsync(responseStreams);

        if (appOptions.PrintCostEnabled)
        {
            PrintChatCost(chatSession.ChatHistory.HistoryItems.Last());
        }

        // Check if more context is needed
        if (codeAssistantManager.CheckExtraContextForResponse(responseContent, out var requiredFiles))
        {
            var confirmation = spectreUtilities.ConfirmationPrompt(
                $"Do you want to add ${string.Join(", ", requiredFiles.Select(file => $"'{file}'"))} to the context?"
            );

            if (confirmation)
            {
                await codeAssistantManager.AddOrUpdateCodeFilesToCache(requiredFiles);
                var fullFilesContentForContext = await codeAssistantManager.GetCodeTreeContentsFromCache(requiredFiles);

                var newQueryWithAddedFiles = promptManager.FilesAddedToChat(fullFilesContentForContext);
                spectreUtilities.SuccessText(
                    $"{string.Join(",", requiredFiles.Select(file => $"'{file}'"))} added to the context."
                );

                await ExecuteAsync(scope, newQueryWithAddedFiles);
            }
        }

        var diffResults = codeAssistantManager.ParseDiffResults(responseContent, appOptions.ContextWorkingDirectory);

        foreach (var diffResult in diffResults)
        {
            var confirmation = spectreUtilities.ConfirmationPrompt(
                $"Do you accept the changes for '{diffResult.ModifiedPath}'?"
            );

            if (confirmation)
            {
                codeAssistantManager.ApplyChanges([diffResult], appOptions.ContextWorkingDirectory);
                await codeAssistantManager.AddOrUpdateCodeFilesToCache([diffResult.ModifiedPath]);
            }
        }

        return true;
    }

    private void PrintChatCost(ChatHistoryItem lastChatHistoryItem)
    {
        if (lastChatHistoryItem.ChatCost is null)
            return;
        spectreUtilities.WriteRule();
        spectreUtilities.InformationText(message: lastChatHistoryItem.ChatCost.ToString(), justify: Justify.Right);
    }
}
