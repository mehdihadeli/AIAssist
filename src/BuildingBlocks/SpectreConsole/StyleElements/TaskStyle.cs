using System.Text.Json.Serialization;

namespace BuildingBlocks.SpectreConsole.StyleElements;

public class TaskStyle : StyleBase
{
    [JsonPropertyName("ticked")]
    public string? Ticked { get; set; }

    [JsonPropertyName("unticked")]
    public string? UnTicked { get; set; }

    [JsonPropertyName("prefix_foreground")]
    public string? PrefixForeground { get; set; }
}
