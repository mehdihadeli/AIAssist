using System.Text.Json.Serialization;

namespace BuildingBlocks.SpectreConsole.StyleElements;

public class BlockQuoteStyle : StyleBase
{
    [JsonPropertyName("prefix_foreground")]
    public string PrefixForeground { get; set; } = default!;
}
