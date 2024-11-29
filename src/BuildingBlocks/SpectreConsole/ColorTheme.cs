using System.Text.Json.Serialization;
using BuildingBlocks.SpectreConsole.StyleElements;

namespace BuildingBlocks.SpectreConsole;

public class ColorTheme
{
    public string Name { get; set; } = default!;

    public string? Foreground { get; set; } = default!;

    [JsonPropertyName("console")]
    public ConsoleStyle ConsoleStyle { get; set; } = default!;

    [JsonPropertyName("code")]
    public CodeStyle CodeStyle { get; set; } = default!;

    [JsonPropertyName("list")]
    public ListStyle ListStyle { get; set; } = default!;

    [JsonPropertyName("block_quote")]
    public BlockQuoteStyle BlockQuoteStyle { get; set; } = default!;

    [JsonPropertyName("head")]
    public HeadStyle HeadStyle { get; set; } = default!;

    [JsonPropertyName("paragraph")]
    public ParagraphStyle ParagraphStyle { get; set; } = default!;

    [JsonPropertyName("emph")]
    public EmphStyle EmphStyle { get; set; } = default!;

    [JsonPropertyName("task")]
    public TaskStyle TaskStyle { get; set; } = default!;

    [JsonPropertyName("colors")]
    public IList<ColorInfo> Colors { get; set; } = default!;

    [JsonPropertyName("link")]
    public LinkStyle LinkStyle { get; set; } = default!;

    [JsonPropertyName("code_block")]
    public CodeBlockStyle CodeBlockStyle { get; set; } = default!;
}
