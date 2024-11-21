using System.ComponentModel;
using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Clients.Contracts;
using Clients.Models;
using Clients.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    ICodeAssistInternalCommands internalCommands,
    IInternalCommandProcessor internalCommandProcessor,
    IOptions<LLMOptions> llmOptions,
    IOptions<AppOptions> appOptions
) : AsyncCommand<CodeAssistCommand.Settings>
{
    private readonly LLMOptions _llmOptions = llmOptions.Value;
    private readonly AppOptions _appOptions = appOptions.Value;
    private static bool _running = true;

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-c|--context-path")]
        [Description("[grey] code context, and the working directory relative to executing command root path.[/].")]
        public string? ContextWorkingDirectory { get; set; }

        [CommandOption("--disable-auto-context")]
        [Description("[grey] disable auto adding all files to the context.[/].")]
        public bool DisableAutoContext { get; set; }

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

        [CommandOption("--chat-api-key <key>")]
        [Description("[grey] the chat model api key.[/].")]
        public string? ChatModelApiKey { get; set; }

        [CommandOption("--embeddings-api-key <key>")]
        [Description("[grey] the embeddings model api key.[/].")]
        public string? EmbeddingsModelApiKey { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var chatSession = chatSessionManager.CreateNewSession();
        chatSessionManager.SetCurrentActiveSession(chatSession);
        internalCommandProcessor.AddCommands(internalCommands);

        using var scope = serviceScopeFactory.CreateScope();
        var codeAssistantManager = scope.ServiceProvider.GetRequiredService<ICodeAssistantManager>();

        spectreUtilities.InformationText("Code assist mode is activated!");
        spectreUtilities.InformationText("Please 'Ctrl+H' to see all available commands in the code assist mode.");
        spectreUtilities.WriteRule();

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

        while (_running)
        {
            spectreUtilities.WriteCursor();

            string? userInput;

            if (spectreUtilities.PressedShortcutKey(internalCommands, ConsoleModifiers.Control, out var pressedKey))
            {
                userInput = pressedKey;
                console.WriteLine(pressedKey);
            }
            else
            {
                console.Write(pressedKey);
                userInput = spectreUtilities.UserPrompt()!;
                userInput = string.Concat(pressedKey, userInput);
            }

            if (string.IsNullOrEmpty(userInput))
            {
                spectreUtilities.ErrorText("Input can't be null or empty string.");
                continue;
            }

            console.Write(new Rule());

            userInput = "can you remove all comments from Add.cs file?";
            _running = await internalCommandProcessor.ProcessCommand(userInput, scope);
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

        if (!string.IsNullOrEmpty(settings.ChatModelApiKey))
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.ChatModel);
            var chatModel = cacheModels.GetModel(_llmOptions.ChatModel);
            chatModel.ModelOption.ApiKey = settings.ChatModelApiKey.Trim();
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingsModelApiKey))
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.EmbeddingsModel);
            var embeddingModel = cacheModels.GetModel(_llmOptions.EmbeddingsModel);
            embeddingModel.ModelOption.ApiKey = settings.EmbeddingsModelApiKey.Trim();
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
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.ChatModel);
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
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.ChatModel);
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
}

public interface ICodeAssistInternalCommands : IList<IInternalConsoleCommand>;

public class CodeAssistInternalCommands : List<IInternalConsoleCommand>, ICodeAssistInternalCommands;
