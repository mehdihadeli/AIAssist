using System.ComponentModel;
using AIAssistant.Models;
using Clients;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIAssistant.Commands;

[Description("Provide explaination for the code.")]
public class CodeExplanationCommand : Command<CodeExplanationCommand.Settings>
{
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
        [DefaultValue(ClientsConstants.Ollama.ChatModels.Deepseek_Coder_V2)]
        public string? ChatModel { get; set; }

        [CommandOption("-t|--code-assist-type <DiffTool>")]
        [Description("[grey] the type of code assist. it can be `embedding` or `summary`.[/].")]
        [DefaultValue(Models.CodeAssistType.Embedding)]
        public CodeAssistType CodeAssistType { get; set; }

        [CommandOption("-e|--embedding-model <Embedding-Chat-Model>")]
        [Description("[grey] llm model for embedding purpose. for example llama3.1.[/].")]
        [DefaultValue(ClientsConstants.Ollama.EmbeddingsModels.Mxbai_Embed_Large)]
        public string? EmbeddingModel { get; set; }

        [CommandOption("-f|--files <Files>")]
        [Description("[grey] the list of files to add the context.[/].")]
        public IEnumerable<string>? Files { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[green]Interpretation process activated![/]");

        return 0;
    }
}
