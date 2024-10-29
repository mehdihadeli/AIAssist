using System.ComponentModel;
using System.Text;
using AIAssistant.Contracts;
using AIAssistant.Contracts.CodeAssist;
using AIAssistant.Models;
using AIAssistant.Models.Options;
using BuildingBlocks.SpectreConsole;
using Clients.Chat.Models;
using Clients.Options;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssistant.Commands;

[Description("Provide code assistance or enhance existing code or add some new features to our application context.")]
public class CodeAssistCommand(
    ICodeAssistantManager codeAssistantManager,
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

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[grey]code assist mode is activated![/]");

        AnsiConsole.MarkupLine("[grey]Please Ctrl+C to exit from code assistant mode.[/]");
        AnsiConsole.Write(new Rule());

        // Handle Ctrl+C to exit gracefully
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            _running = false;

            AnsiConsole.MarkupLine("[red]process interrupted. Exiting...[/]");

            eventArgs.Cancel = true; // Prevent immediate termination
        };

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
            _options.Files = settings.Files;
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
            //var userRequest = ReadInput("Please enter your request to apply on your code base:");
            var userRequest = "can you remove all comments in Add.cs file?";

            var responseStreams = codeAssistantManager.QueryAsync(userRequest);
            var responseContent = await CollectAndWriteStreamResponseAsync(responseStreams);

            var changesCodeBlocks = codeAssistantManager.ParseResponseCodeBlocks(responseContent);

            foreach (var changesCodeBlock in changesCodeBlocks)
            {
                var confirmation = AnsiConsole.Prompt(
                    new TextPrompt<bool>(
                        $"[lightsteelblue]Do you accept the changes for `{changesCodeBlock.FilePath}`?[/]"
                    )
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(true)
                        .WithConverter(choice => choice ? "y" : "n")
                );

                if (confirmation)
                {
                    codeAssistantManager.ApplyChangesToFiles([changesCodeBlock]);
                }
            }

            // Output result after processing
            AnsiConsole.MarkupLine($"[seagreen1]Request '{userRequest}' processed successfully![/]");
            Thread.Sleep(1000); // Delay before asking again
        }

        return 0;
    }

    private async Task<string> CollectAndWriteStreamResponseAsync(IAsyncEnumerable<string?> responseStreams)
    {
        var printer = new StreamPrinter(AnsiConsole.Console, useMarkdown: true);
        var result = await printer.PrintAsync(responseStreams);

        return result;
    }

    private string ReadInput(string prompt)
    {
        string input;
        while (true)
        {
            input = AnsiConsole.Prompt(
                new TextPrompt<string>($"[lightsteelblue]{prompt}[/]").PromptStyle(new Style(Color.White))
            );

            // Check if the input is not null or empty
            if (!string.IsNullOrWhiteSpace(input))
            {
                break; // Exit the loop when a valid input is given
            }
            AnsiConsole.WriteLine("Invalid input, please try again.");
        }
        return input;
    }
}
