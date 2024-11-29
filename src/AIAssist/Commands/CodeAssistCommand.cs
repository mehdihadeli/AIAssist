using System.ComponentModel;
using AIAssist.Contracts;
using AIAssist.Contracts.CodeAssist;
using AIAssist.Models.Options;
using BuildingBlocks.SpectreConsole.Contracts;
using Clients.Contracts;
using Clients.Models;
using Clients.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssist.Commands;

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
    private readonly Model _chatModel =
        cacheModels.GetModel(llmOptions.Value.ChatModel)
        ?? throw new ArgumentNullException($"Model '{llmOptions.Value.ChatModel}' not found in the ModelCache.");
    private readonly Model? _embeddingModel = cacheModels.GetModel(llmOptions.Value.EmbeddingsModel);

    private static bool _running = true;

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-c|--context-path")]
        [Description("[grey] code context, and the working directory relative to executing command root path.[/].")]
        public string? ContextWorkingDirectory { get; set; }

        [CommandOption("--disable-auto-context")]
        [Description("[grey] disable auto adding all files to the context.[/].")]
        public bool DisableAutoContext { get; set; }

        [CommandOption("-m|--chat-model <chat-model>")]
        [Description("[grey] llm model for chatting with ai. for example llama3.1.[/].")]
        public string? ChatModel { get; set; }

        [CommandOption("-e|--embedding-model <embedding-model>")]
        [Description("[grey] llm model for embedding purpose. for example mxbai_embed_large.[/].")]
        public string? EmbeddingModel { get; set; }

        [CommandOption("-f|--files <files>")]
        [Description("[grey] the list of files to add the context.[/].")]
        public IList<string>? Files { get; set; }

        [CommandOption("-d|--diff <diff-type>")]
        [Description(
            "[grey] the diff tool for showing changes. it can be `unified-diff`, `code-block-diff` and `search-replace-diff`.[/]."
        )]
        public CodeDiffType? CodeDiffType { get; set; }

        [CommandOption("-t|--code-assist-type <code-assist-type>")]
        [Description("[grey] the type of code assist. it can be `embedding` or `summary`.[/].")]
        public CodeAssistType? CodeAssistType { get; set; }

        [CommandOption("--threshold <threshold>")]
        [Description("[grey] the threshold is a value for using in the `embedding`.[/].")]
        public decimal? Threshold { get; set; }

        [CommandOption("--temperature <temperature>")]
        [Description(
            "[grey] the temperature is a value for controlling creativity or randomness on the llm response.[/]."
        )]
        public decimal? Temperature { get; set; }

        [CommandOption("--chat-api-key <chat-api-key>")]
        [Description("[grey] the chat model api key.[/].")]
        public string? ChatModelApiKey { get; set; }

        [CommandOption("--embeddings-api-key <key>")]
        [Description("[grey] the embeddings model api key.[/].")]
        public string? EmbeddingsModelApiKey { get; set; }

        [CommandOption("--chat-api-version <version>")]
        [Description("[grey] the chat model api version.[/].")]
        public string? ChatApiVersion { get; set; }

        [CommandOption("--chat-deployment-id <deployment-id>")]
        [Description("[grey] the chat model deployment-id.[/].")]
        public string? ChatDeploymentId { get; set; }

        [CommandOption("--chat-base-address <base-address>")]
        [Description("[grey] the chat model base-address.[/].")]
        public string? ChatBaseAddress { get; set; }

        [CommandOption("--embeddings-api-version <version>")]
        [Description("[grey] the embeddings model api version.[/].")]
        public string? EmbeddingsApiVersion { get; set; }

        [CommandOption("--embeddings-deployment-id <deployment-id>")]
        [Description("[grey] the embeddings model deployment-id.[/].")]
        public string? EmbeddingsDeploymentId { get; set; }

        [CommandOption("--embeddings-base-address <base-address>")]
        [Description("[grey] the embeddings model base-address.[/].")]
        public string? EmbeddingsBaseAddress { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var chatSession = chatSessionManager.CreateNewSession();
        chatSessionManager.SetCurrentActiveSession(chatSession);
        internalCommandProcessor.AddCommands(internalCommands);

        using var scope = serviceScopeFactory.CreateScope();
        var codeAssistantManager = scope.ServiceProvider.GetRequiredService<ICodeAssistantManager>();

        SetupOptions(settings);

        spectreUtilities.SummaryTextLine("Code assist mode is activated!");
        spectreUtilities.NormalText("Chat model: ");
        spectreUtilities.HighlightTextLine(_chatModel.Name);

        spectreUtilities.NormalText("Embedding model: ");
        spectreUtilities.HighlightTextLine(_embeddingModel?.Name ?? "-");

        spectreUtilities.NormalText("CodeAssistType: ");
        spectreUtilities.HighlightTextLine(_chatModel.CodeAssistType.ToString());

        spectreUtilities.NormalText("CodeDiffType: ");
        spectreUtilities.HighlightTextLine(_chatModel.CodeDiffType.ToString());

        spectreUtilities.NormalTextLine(
            "Please 'Ctrl+H' to see all available commands in the code assist mode.",
            decoration: Decoration.Bold
        );
        spectreUtilities.WriteRule();

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
                spectreUtilities.ErrorTextLine("Input can't be null or empty string.");
                continue;
            }

            console.Write(new Rule());

            //userInput = "can you remove all comments from Add.cs file?";
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
            _chatModel.ApiKey = settings.ChatModelApiKey.Trim();
        }

        if (!string.IsNullOrEmpty(settings.ChatApiVersion))
        {
            _chatModel.ApiVersion = settings.ChatApiVersion.Trim();
        }

        if (!string.IsNullOrEmpty(settings.ChatDeploymentId))
        {
            _chatModel.DeploymentId = settings.ChatDeploymentId.Trim();
        }

        if (!string.IsNullOrEmpty(settings.ChatBaseAddress))
        {
            _chatModel.BaseAddress = settings.ChatBaseAddress.Trim();
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingsModelApiKey) && _embeddingModel is not null)
        {
            _embeddingModel.ApiKey = settings.EmbeddingsModelApiKey.Trim();
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingsApiVersion) && _embeddingModel is not null)
        {
            _embeddingModel.ApiVersion = settings.EmbeddingsApiVersion.Trim();
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingsDeploymentId) && _embeddingModel is not null)
        {
            _embeddingModel.DeploymentId = settings.EmbeddingsDeploymentId.Trim();
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingsBaseAddress) && _embeddingModel is not null)
        {
            _embeddingModel.BaseAddress = settings.EmbeddingsBaseAddress.Trim();
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

        if (settings.CodeDiffType is not null)
        {
            _llmOptions.CodeDiffType = settings.CodeDiffType.Value;
            _chatModel.CodeDiffType = settings.CodeDiffType.Value;

            if (_embeddingModel != null)
                _embeddingModel.CodeDiffType = settings.CodeDiffType.Value;
        }

        if (settings.CodeAssistType is not null)
        {
            _llmOptions.CodeAssistType = settings.CodeAssistType.Value;
            _chatModel.CodeAssistType = settings.CodeAssistType.Value;

            if (_embeddingModel != null)
                _embeddingModel.CodeAssistType = settings.CodeAssistType.Value;
        }

        if (settings.Threshold is not null)
        {
            _llmOptions.Threshold = settings.Threshold.Value;
            _chatModel.Threshold = settings.Threshold.Value;

            if (_embeddingModel != null)
                _embeddingModel.Threshold = settings.Threshold.Value;
        }

        if (settings.Temperature is not null)
        {
            _llmOptions.Temperature = settings.Temperature.Value;
            _chatModel.Temperature = settings.Temperature.Value;

            if (_embeddingModel != null)
                _embeddingModel.Temperature = settings.Temperature.Value;
        }
    }
}

public interface ICodeAssistInternalCommands : IList<IInternalConsoleCommand>;

public class CodeAssistInternalCommands : List<IInternalConsoleCommand>, ICodeAssistInternalCommands;
