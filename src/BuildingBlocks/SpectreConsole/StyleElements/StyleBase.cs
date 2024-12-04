using System.Text.Json.Serialization;

namespace BuildingBlocks.SpectreConsole.StyleElements;

public class StyleBase
{
    [JsonPropertyName("background")]
    public string? Background { get; set; } = default!;

    [JsonPropertyName("foreground")]
    public string? Foreground { get; set; } = default!;

    [JsonPropertyName("margin")]
    public int Margin { get; set; }

    [JsonPropertyName("bold")]
    public bool Bold { get; set; }

    [JsonPropertyName("italic")]
    public bool Italic { get; set; }

    [JsonPropertyName("underline")]
    public bool Underline { get; set; }

    public StyleBase CombineStyle(Action<StyleBase> styleAction)
    {
        styleAction(this);

        return this;
    }
}
