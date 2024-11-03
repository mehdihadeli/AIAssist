using System.ComponentModel;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Models;
using AIAssistant.Models.Options;
using BuildingBlocks.SpectreConsole;
using BuildingBlocks.SpectreConsole.Contracts;
using Clients.Chat.Models;
using Clients.Options;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssistant.Commands;

[Description("Provide code assistance or enhance existing code or add some new features to our application context.")]
public class CodeAssistCommand(
    ICodeAssistantManager codeAssistantManager,
    ISpectreConsoleUtilities spectreConsoleUtilities,
    IAnsiConsole console,
    IOptions<CodeAssistOptions> options,
    IOptions<LLMOptions> llmOptions
) : AsyncCommand<CodeAssistCommand.Settings>
{
    private readonly CodeAssistOptions _options = options.Value;
    private readonly LLMOptions _llmOptions = llmOptions.Value;
    private static bool _running = true;

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

    static async IAsyncEnumerable<string> GetMarkdownLinesAsync2()
    {
        yield return "```csharp\nConsole.Write('Hello World');\n";
        yield return "Console.Write('Hello World');\n";
        yield return "Console.Write('Hello World');\n";
        yield return "Console.Write('Hello World');\n";
        yield return "Console.Write('Hello World');\n";
        yield return "```";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        // var printer = new StreamPrinter(console, true);
        // await printer.PrintAsync(GetMarkdownLinesAsync2());

        spectreConsoleUtilities.InformationText("Code assist mode is activated!");
        spectreConsoleUtilities.InformationText("Please 'Ctrl+C' to exit from code assistant mode.");
        console.Write(new Rule());

        // Handle Ctrl+C to exit gracefully
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            _running = false;
            spectreConsoleUtilities.ErrorText("Process interrupted. Exiting...");
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
                    var session = new ChatSession();

                    await codeAssistantManager.LoadCodeFiles(session, _options.ContextWorkingDirectory, _options.Files);
                }
            );

        // Run in a loop until Ctrl+C is pressed
        while (_running)
        {
            //var userRequest = spectreConsoleUtilities.UserPrompt("Please enter your request to apply on your code base:");
            var userRequest = "can you remove all comments in Add.cs file?";

            var responseStreams = codeAssistantManager.QueryAsync(userRequest);
            var responseContent = await CollectAndWriteStreamResponseAsync(responseStreams);

            var changesCodeBlocks = codeAssistantManager.ParseResponseCodeBlocks(responseContent);

            foreach (var changesCodeBlock in changesCodeBlocks)
            {
                var confirmation = spectreConsoleUtilities.ConfirmationPrompt(
                    $"Do you accept the changes for `{changesCodeBlock.FilePath}`?"
                );

                if (confirmation)
                {
                    codeAssistantManager.ApplyChangesToFiles([changesCodeBlock], options.Value.ContextWorkingDirectory);

                    spectreConsoleUtilities.SuccessText(
                        $"changes applied successfully on '{changesCodeBlock.FilePath}' file!"
                    );
                }
            }

            var goNext = spectreConsoleUtilities.ConfirmationPrompt("Do you want to continue");
            if (goNext)
                break;
        }

        Console.ReadKey();

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

        _options.ContextWorkingDirectory = !string.IsNullOrEmpty(settings.ContextWorkingDirectory)
            ? Path.Combine(Directory.GetCurrentDirectory(), settings.ContextWorkingDirectory)
            : Directory.GetCurrentDirectory(); // set to current working directory

        if (settings.Files is not null && settings.Files.Any())
        {
            _options.Files = settings
                .Files.Select(file => Path.Combine(_options.ContextWorkingDirectory, file))
                .ToList();
        }

        if (settings.DisableAutoContext)
        {
            _options.AutoContextEnabled = false;
        }

        switch (settings.CodeDiff)
        {
            case CodeDiffType.UnifiedDiff:
                _options.CodeDiffType = CodeDiffType.UnifiedDiff;
                break;
            case CodeDiffType.CodeBlockDiff:
                _options.CodeDiffType = CodeDiffType.CodeBlockDiff;
                break;
        }

        switch (settings.CodeAssistType)
        {
            case CodeAssistType.Embedding:
                _options.CodeAssistType = CodeAssistType.Embedding;
                break;
            case CodeAssistType.Summary:
                _options.CodeAssistType = CodeAssistType.Summary;
                break;
        }
    }

    private async Task<string> CollectAndWriteStreamResponseAsync(IAsyncEnumerable<string?> responseStreams)
    {
        var printer = new StreamPrinter(AnsiConsole.Console, useMarkdown: true);
        var result = await printer.PrintAsync(responseStreams);

        return result;
    }
}
