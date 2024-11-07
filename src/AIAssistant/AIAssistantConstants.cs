namespace AIAssistant;

public class AIAssistantConstants
{
    public const string PromptsTemplatesNamespace = "Prompts.Templates";

    public class Prompts
    {
        public const string CodeBlockTemplate = "code-block";
        public const string CodeEmbeddingTemplate = "code-embedding";
        public const string CodeAssistantUnifiedDiffTemplate = "code-assistant-unified-diff";
        public const string CodeAssistantCodeBlockdDiffTemplate = "code-assistant-code-block-diff";
        public const string CodeAssistantMergeConflictDiffTemplate = "code-assist-merge-conflict-diff";
    }

    public class InternalCommands
    {
        public const string AddFiles = "add_files";
        public const string ClearHistory = "clear_history";
        public const string Tokens = "tokens";
        public const string Clear = "clear";
        public const string Exit = "exit";
        public const string Help = "help";
        public const string Summarize = "summarize";
    }

    public class Configurations
    {
        public const string ThemeName = "ThemeName";
    }
}
