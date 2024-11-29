namespace Clients.Models;

public class Model
{
    /// <summary>
    /// LLM compatible model name without an AI provider type with '/' prefix
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Model name with an AI provider type with '/' prefix
    /// </summary>
    public string OriginalName { get; set; } = default!;
    public AIProvider AIProvider { get; set; }
    public ModelType ModelType { get; set; }
    public CodeDiffType CodeDiffType { get; set; }
    public CodeAssistType CodeAssistType { get; set; }
    public decimal Threshold { get; set; }
    public decimal Temperature { get; set; }
    public string? BaseAddress { get; set; }
    public string? ApiVersion { get; set; }
    public string? DeploymentId { get; set; }
    public string? ApiKey { get; set; }
    public int MaxTokens { get; set; }
    public int MaxInputTokens { get; set; }
    public int MaxOutputTokens { get; set; }
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
