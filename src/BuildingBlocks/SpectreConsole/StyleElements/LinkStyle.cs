using System.Text.Json.Serialization;

namespace BuildingBlocks.SpectreConsole.StyleElements;

public class LinkStyle : StyleBase
{
    [JsonPropertyName("link_foreground")]
    public string? LinkForeground { get; set; } = default!;

    [JsonPropertyName("link_text_foreground")]
    public string? LinkTextForeground { get; set; } = default!;
}
