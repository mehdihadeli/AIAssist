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

        [CommandOption("--threshold <threshold")]
        [Description("[grey] the threshold is a value for using in the `embedding`.[/].")]
        public decimal? Threshold { get; set; }

        [CommandOption("--temperature <temperature")]
        [Description(
            "[grey] the temperature is a value for controlling creativity or randomness on the llm response.[/]."
        )]
        public decimal? Temperature { get; set; }

        [CommandOption("--chat-api-key <key>")]
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

        if (!string.IsNullOrEmpty(settings.ChatApiVersion))
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.ChatModel);
            var chatModel = cacheModels.GetModel(_llmOptions.ChatModel);
            chatModel.ModelOption.ApiVersion = settings.ChatApiVersion.Trim();
        }

        if (!string.IsNullOrEmpty(settings.ChatDeploymentId))
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.ChatModel);
            var chatModel = cacheModels.GetModel(_llmOptions.ChatModel);
            chatModel.ModelOption.DeploymentId = settings.ChatDeploymentId.Trim();
        }

        if (!string.IsNullOrEmpty(settings.ChatBaseAddress))
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.ChatModel);
            var chatModel = cacheModels.GetModel(_llmOptions.ChatModel);
            chatModel.ModelOption.BaseAddress = settings.ChatBaseAddress.Trim();
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingsModelApiKey))
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.EmbeddingsModel);
            var embeddingModel = cacheModels.GetModel(_llmOptions.EmbeddingsModel);
            embeddingModel.ModelOption.ApiKey = settings.EmbeddingsModelApiKey.Trim();
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingsApiVersion))
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.EmbeddingsModel);
            var embeddingModel = cacheModels.GetModel(_llmOptions.EmbeddingsModel);
            embeddingModel.ModelOption.ApiVersion = settings.EmbeddingsApiVersion.Trim();
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingsDeploymentId))
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.EmbeddingsModel);
            var embeddingModel = cacheModels.GetModel(_llmOptions.EmbeddingsModel);
            embeddingModel.ModelOption.DeploymentId = settings.EmbeddingsDeploymentId.Trim();
        }

        if (!string.IsNullOrEmpty(settings.EmbeddingsBaseAddress))
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.EmbeddingsModel);
            var embeddingModel = cacheModels.GetModel(_llmOptions.EmbeddingsModel);
            embeddingModel.ModelOption.BaseAddress = settings.EmbeddingsBaseAddress.Trim();
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
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.ChatModel);
            var chatModel = cacheModels.GetModel(_llmOptions.ChatModel);
            chatModel.ModelOption.CodeDiffType = settings.CodeDiffType.Value;
        }

        if (settings.CodeAssistType is not null)
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.ChatModel);
            var chatModel = cacheModels.GetModel(_llmOptions.ChatModel);
            chatModel.ModelOption.CodeAssistType = settings.CodeAssistType.Value;
        }

        if (settings.Threshold is not null)
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.EmbeddingsModel);
            var embeddingsModel = cacheModels.GetModel(_llmOptions.EmbeddingsModel);
            embeddingsModel.ModelOption.Threshold = settings.Threshold.Value;
        }

        if (settings.Temperature is not null)
        {
            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.ChatModel);
            var chatModel = cacheModels.GetModel(_llmOptions.ChatModel);
            chatModel.ModelOption.Temperature = settings.Temperature.Value;

            ArgumentException.ThrowIfNullOrEmpty(_llmOptions.EmbeddingsModel);
            var embeddingsModel = cacheModels.GetModel(_llmOptions.EmbeddingsModel);
            embeddingsModel.ModelOption.Temperature = settings.Temperature.Value;
        }
    }
}

public interface ICodeAssistInternalCommands : IList<IInternalConsoleCommand>;

public class CodeAssistInternalCommands : List<IInternalConsoleCommand>, ICodeAssistInternalCommands;
