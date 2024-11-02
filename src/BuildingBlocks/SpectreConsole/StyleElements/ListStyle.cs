using System.Text.Json.Serialization;

namespace BuildingBlocks.SpectreConsole.StyleElements;

public class ListStyle : StyleBase
{
    [JsonPropertyName("prefix_foreground")]
    public string? PrefixForeground { get; set; } = default!;

    [JsonPropertyName("block_prefix")]
    public string? BlockPrefix { get; set; } = default!;
}
