namespace Clients.Models;

public class ModelOption
{
    public CodeDiffType? CodeDiffType { get; set; }
    public CodeAssistType? CodeAssistType { get; set; }
    public decimal? Threshold { get; set; }
    public decimal? Temperature { get; set; }
    public string? BaseAddress { get; set; }
    public string? ApiVersion { get; set; }
    public string? DeploymentId { get; set; }
    public string? ApiKey { get; set; }
}
