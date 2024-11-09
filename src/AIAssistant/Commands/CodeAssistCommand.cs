using System.ComponentModel;
using AIAssistant.Chat.Models;
using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Models.Options;
using AIAssistant.Prompts;
using BuildingBlocks.SpectreConsole;
using BuildingBlocks.SpectreConsole.Contracts;
using Clients.Contracts;
using Clients.Models;
using Clients.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssistant.Commands;

[Description("Provide code assistance or enhance existing code or add some new features to our application context.")]
public class CodeAssistCommand(
    IServiceScopeFactory serviceScopeFactory,
    ISpectreUtilities spectreUtilities,
    IAnsiConsole console,
    IChatSessionManager chatSessionManager,
    ICacheModels cacheModels,
    IOptions<LLMOptions> llmOptions,
    IOptions<AppOptions> appOptions
) : AsyncCommand<CodeAssistCommand.Settings>
{
    private readonly LLMOptions _llmOptions = llmOptions.Value;
    private readonly AppOptions _appOptions = appOptions.Value;
    private static bool _running = true;

    private readonly IList<IInternalConsoleCommand> _internalConsoleCommands =
    [
        new InternalConsoleCommand(AIAssistantConstants.InternalCommands.AddFiles, ":a", ConsoleKey.A),
        new InternalConsoleCommand(AIAssistantConstants.InternalCommands.Tokens, ":t", ConsoleKey.T),
        new InternalConsoleCommand(AIAssistantConstants.InternalCommands.ClearHistory, ":c", ConsoleKey.C),
        new InternalConsoleCommand(AIAssistantConstants.InternalCommands.Exit, ":e", ConsoleKey.E),
        new InternalConsoleCommand(AIAssistantConstants.InternalCommands.Help, ":h", ConsoleKey.H),
        new InternalConsoleCommand(AIAssistantConstants.InternalCommands.Summarize, ":s", ConsoleKey.S),
    ];

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-c|--context-path")]
        [Description("[grey] code context, and the working directory relative to executing command root path.[/].")]
        public string? ContextWorkingDirectory { get; set; }

        [CommandOption("--disable-auto-context")]
        [Description("[grey] disable auto adding all files to the context.[/].")]
        public bool DisableAutoContext { get; set; }

        [CommandOption("-s|--summarize-history")]
        [Description("[grey] summarize history by llm for deacreasing consumption tkoen.[/].")]
        public bool Summarize { get; set; }

        [CommandOption("-m|--chat-model <Chat-Model>")]
        [Description("[grey] llm model for chatting with ai. for example llama3.1.[/].")]
        public string? ChatModel { get; set; }

        [CommandOption("-t|--code-assist-type <DiffTool>")]
        [Description("[grey] the type of code assist. it can be `embedding` or `summary`.[/].")]
        public CodeAssistType? CodeAssistType { get; set; }

        [CommandOption("-e|--embedding-model <Embedding-Chat-Model>")]
        [Description("[grey] llm model for embedding purpose. for example mxbai_embed_large.[/].")]
        public string? EmbeddingModel { get; set; }

        [CommandOption("-f|--files <Files>")]
        [Description("[grey] the list of files to add the context.[/].")]
        public IList<string>? Files { get; set; }

        [CommandOption("-d|--diff <Diff-Strategy>")]
        [Description(
            "[grey] the diff tool for showing changes. it can be `unifieddiff`, `codeblockdiff` and `mergeconflictdiff`.[/]."
        )]
        public CodeDiffType? CodeDiff { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var chatSession = chatSessionManager.CreateNewSession();
        chatSessionManager.SetCurrentActiveSession(chatSession);
        using var scope = serviceScopeFactory.CreateScope();
        var codeAssistantManager = scope.ServiceProvider.GetRequiredService<ICodeAssistantManager>();

        spectreUtilities.InformationText("Code assist mode is activated!");
        spectreUtilities.InformationText("Please 'Shift+H' to see all available commands in the code assist mode.");
        console.Write(new Rule());

        // Handle Ctrl+C to exit gracefully
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            ExitCommand();
        };

        SetupOptions(settings);

        await AnsiConsole
            .Console.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("deepskyblue1 bold"))
            .StartAsync(
                "initializing...",
                async _ =>
                {
                    await codeAssistantManager.LoadCodeFiles(_appOptions.ContextWorkingDirectory, _appOptions.Files);
                }
            );

        // Run in a loop until Ctrl+C is pressed
        while (_running)
        {
            // spectreUtilities.WriteCursor();
            //
            // string? userInput;
            //
            // if (
            //     spectreUtilities.PressedShortcutKey(
            //         _internalConsoleCommands,
            //         ConsoleModifiers.Shift,
            //         out var pressedKey
            //     )
            // )
            // {
            //     userInput = pressedKey;
            //     console.WriteLine(pressedKey);
            // }
            // else
            // {
            //     console.Write(pressedKey);
            //     userInput = spectreUtilities.UserPrompt()!;
            //     userInput = string.Concat(pressedKey, userInput);
            // }
            //
            // if (string.IsNullOrEmpty(userInput))
            // {
            //     spectreUtilities.ErrorText("Input can't be null or empty string.");
            //     continue;
            // }
            //
            // console.Write(new Rule());

            var userInput = spectreUtilities.UserPrompt();
            await HandleSpecialCommands(userInput, chatSession, codeAssistantManager);
        }

        chatSessionManager.SetCurrentActiveSession(null);

        return 0;
    }

    private void SetupOptions(Settings settings)
    {
        if (!string.IsNullOrEmpty(settings.ChatModel))
        {
            _llmOptions.ChatModel = settings.ChatModel;
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingModel))
        {
            _llmOptions.EmbeddingsModel = settings.EmbeddingModel;
        }

        _appOptions.ContextWorkingDirectory = !string.IsNullOrEmpty(settings.ContextWorkingDirectory)
            ? Path.Combine(Directory.GetCurrentDirectory(), settings.ContextWorkingDirectory)
            : Directory.GetCurrentDirectory(); // set to current working directory

        if (settings.Files is not null && settings.Files.Any())
        {
            _appOptions.Files = settings
                .Files.Select(file => Path.Combine(_appOptions.ContextWorkingDirectory, file))
                .ToList();
        }

        if (settings.DisableAutoContext)
        {
            _appOptions.AutoContextEnabled = false;
        }

        if (settings.CodeDiff is not null)
        {
            var model = cacheModels.GetModel(_llmOptions.ChatModel);

            switch (settings.CodeDiff)
            {
                case CodeDiffType.UnifiedDiff:
                    model.ModelOption.CodeDiffType = CodeDiffType.UnifiedDiff;
                    break;
                case CodeDiffType.CodeBlockDiff:
                    model.ModelOption.CodeDiffType = CodeDiffType.CodeBlockDiff;
                    break;
            }
        }

        if (settings.CodeAssistType is not null)
        {
            var model = cacheModels.GetModel(_llmOptions.ChatModel);
            switch (settings.CodeAssistType)
            {
                case CodeAssistType.Embedding:
                    model.ModelOption.CodeAssistType = CodeAssistType.Embedding;
                    break;
                case CodeAssistType.Summary:
                    model.ModelOption.CodeAssistType = CodeAssistType.Summary;
                    break;
            }
        }
    }

    private async Task HandleSpecialCommands(
        string userInput,
        ChatSession chatSession,
        ICodeAssistantManager codeAssistantManager
    )
    {
        switch (userInput)
        {
            case { } add when add.StartsWith(":add_files") || add.StartsWith(":a"):
                var arguments = spectreUtilities.GetArguments(add);
                AddFilesCommand(arguments);
                break;
            case ":clear_history":
            case ":c":
                ClearHistoryCommand();
                break;
            case ":tree":
                ContextTreeListCommand();
                break;
            case ":token":
            case ":t":
                ShowTokenCommand();
                break;
            default:
                var userRequest = "can you remove all comments in Add.cs file?";
                //var userRequest = spectreConsoleUtilities.UserPrompt("Please enter your request to apply on your code base:");
                await RunCommand(userRequest, chatSession, codeAssistantManager);
                break;
        }
    }

    private async Task RunCommand(string userInput, ChatSession chatSession, ICodeAssistantManager codeAssistantManager)
    {
        var responseStreams = codeAssistantManager.QueryAsync(userInput);
        var streamPrinter = new StreamPrinter(console, useMarkdown: true);
        var responseContent = await streamPrinter.PrintAsync(responseStreams);

        if (appOptions.Value.PrintCostEnabled)
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

                var newQueryWithAddedFiles = SharedPrompts.FilesAddedToChat(fullFilesContentForContext);
                spectreUtilities.SuccessText(
                    $"{string.Join(",", requiredFiles.Select(file => $"'{file}'"))} added to the context."
                );

                await RunCommand(newQueryWithAddedFiles, chatSession, codeAssistantManager);
            }
        }

        var changesCodeBlocks = codeAssistantManager.ParseResponseCodeBlocks(responseContent);

        foreach (var changesCodeBlock in changesCodeBlocks)
        {
            var confirmation = spectreUtilities.ConfirmationPrompt(
                $"Do you accept the changes for '{changesCodeBlock.FilePath}'?"
            );

            if (confirmation)
            {
                codeAssistantManager.ApplyChangesToFiles([changesCodeBlock], _appOptions.ContextWorkingDirectory);
                await codeAssistantManager.AddOrUpdateCodeFilesToCache([changesCodeBlock.FilePath]);
            }
        }
    }

    private void AddFilesCommand(IEnumerable<string> args)
    {
        var filesToAdd = new List<string>();
        var matcher = new Matcher();

        foreach (var path in args)
        {
            if (Directory.Exists(path))
            {
                // Add all files from the directory recursively
                var directoryFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                filesToAdd.AddRange(directoryFiles);
            }
            else if (path.Contains('*', StringComparison.Ordinal))
            {
                matcher.AddInclude(path);
            }
            else if (File.Exists(path))
            {
                filesToAdd.Add(path);
            }
            else
            {
                spectreUtilities.ErrorText($"The specified path does not exist: {path}");
            }
        }

        var directoryInfo = new DirectoryInfo(_appOptions.ContextWorkingDirectory);
        var directoryInfoWrapper = new DirectoryInfoWrapper(directoryInfo);

        var results = matcher.Execute(directoryInfoWrapper);
        filesToAdd.AddRange(results.Files.Select(f => f.Path));

        foreach (var fileToAdd in filesToAdd.Distinct())
        {
            if (!_appOptions.Files.Contains(fileToAdd))
            {
                _appOptions.Files.Add(fileToAdd);
            }
        }

        spectreUtilities.InformationText(
            filesToAdd.Count != 0 ? $"Files added: {string.Join(", ", filesToAdd)}" : "No files were added."
        );
    }

    private void HelpCommand()
    {
        var helpText =
            @"- `:clear` / `:c` / Ctrl+C - Clear the conversation.
- `:quit` / `:q` / Ctrl+D - Quit the program.
- `:rerun` / `:r` / Ctrl+R - Re-run the last message.
- `:help` / `:h` / `:?` - Show this help message.";
    }

    private void ExitCommand()
    {
        spectreUtilities.ErrorText("Process interrupted. Exiting...");
        _running = false;
    }

    private void ContinueCommand() { }

    private void ClearCommand()
    {
        console.Clear();
    }

    private void ChangeModelCommand() { }

    private void ModelsListCommand() { }

    private void ContextTreeListCommand()
    {
        spectreUtilities.DirectoryTree(_appOptions.ContextWorkingDirectory, 0);
    }

    private void CopyAllChatCommand() { }

    private void CopyLastChatCommand() { }

    private void ClearHistoryCommand()
    {
        spectreUtilities.InformationText("History cleared.");
    }

    private void ShowTokenCommand()
    {
        // Display token information (this is an example; replace with your actual logic)
        var tokenInfo = "Current token limit: 4096"; // Example info
        spectreUtilities.InformationText(tokenInfo);
    }

    private void PrintChatCost(ChatHistoryItem lastChatHistoryItem)
    {
        if (lastChatHistoryItem.ChatCost is null)
            return;
        spectreUtilities.WriteRule();
        spectreUtilities.InformationText(message: lastChatHistoryItem.ChatCost.ToString(), justify: Justify.Right);
    }
}
