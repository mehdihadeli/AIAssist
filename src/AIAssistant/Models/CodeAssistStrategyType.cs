namespace AIAssistant.Models;

public enum CodeAssistStrategyType
{
    /// <summary>
    /// Tree-Sitter definition information with using rag and embedding
    /// </summary>
    Embedding = 0,

    /// <summary>
    /// Tree-Sitter summary information from capturing data
    /// </summary>
    Summary,
}