namespace Clients.Models;

// https://platform.openai.com/docs/models
// https://platform.openai.com/docs/models/gpt-4-and-gpt-4-turbo
// https://docs.anthropic.com/en/docs/about-claude/models

public class ModelInformation
{
    public AIProvider? AIProvider { get; set; }
    public ModelType? ModelType { get; set; }
    public int? MaxTokens { get; set; }
    public int? MaxInputTokens { get; set; }
    public int? MaxOutputTokens { get; set; }
    public decimal InputCostPerToken { get; set; }
    public decimal OutputCostPerToken { get; set; }
    public int? OutputVectorSize { get; set; }
    public bool SupportsFunctionCalling { get; set; }
    public bool SupportsParallelFunctionCalling { get; set; }
    public bool SupportsVision { get; set; }
    public int? EmbeddingDimensions { get; set; }
    public bool SupportsPromptCaching { get; set; }
    public bool Enabled { get; set; } = true;
}
