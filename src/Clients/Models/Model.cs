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
    public ModelInformation ModelInformation { get; set; } = default!;
    public ModelOption ModelOption { get; set; } = default!;
}
