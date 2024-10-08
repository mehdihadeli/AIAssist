using System.Text.Json.Serialization;

namespace Clients.Anthropic.Models;

public class AnthropicCompletionResponse
{
    /// <summary>
    /// Gets or sets the completion message returned from the model.
    /// </summary>
    public string Completion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the completion response.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name that generated the completion.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason for stopping the generation.
    /// </summary>
    [JsonPropertyName("stop_reason")]
    public string StopReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of response, which should indicate it is a completion.
    /// </summary>
    public string Type { get; set; } = string.Empty;
}
