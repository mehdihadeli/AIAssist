namespace AIAssist;

public class AIAssistConstants
{
    public const string PromptsTemplatesNamespace = "Prompts.Templates";

    public class Prompts
    {
        public const string CodeContextTemplate = "code-context";
        public const string CodeBlockTemplate = "code-block";
        public const string AskMoreContext = "ask-more-context";
        public const string CodeEmbeddingTemplate = "code-embedding";
        public const string CodeAssistantUnifiedDiffTemplate = "code-assistant-unified-diff";
        public const string CodeAssistantCodeBlockdDiffTemplate = "code-assistant-code-block-diff";
        public const string CodeAssistantMergeConflictDiffTemplate = "code-assist-merge-conflict-diff";
        public const string CodeAssistantSearchReplaceDiffTemplate = "code-assist-search-replace-diff";
    }

    public class InternalCommands
    {
        public const string Run = "run";
        public const string AddFiles = "add_files";
        public const string TreeList = "tree";
        public const string ClearHistory = "clear_history";
        public const string Tokens = "tokens";
        public const string Clear = "clear";
        public const string Quit = "quit";
        public const string Help = "help";
        public const string Summarize = "summarize";
    }

    public class Configurations
    {
        public const string ThemeName = "ThemeName";
    }
}
