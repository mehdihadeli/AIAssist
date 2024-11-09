using System.Text.Json.Serialization;
using ColorCode.Styling;
using Humanizer;

namespace BuildingBlocks.MarkdigMarkdown;

public class CodeSnippetTheme
{
    [JsonPropertyName("background")]
    public string Background { get; set; } = default!;

    [JsonPropertyName("foreground")]
    public string Foreground { get; set; } = default!;

    [JsonPropertyName("margin")]
    public int Margin { get; set; }

    [JsonPropertyName("token_types")]
    public IDictionary<string, TokenType> TokenTypes { get; set; } = default!;

    public StyleDictionary ToColorCodeStyleDictionary()
    {
        var styleDictionary = new StyleDictionary();

        // Iterate through the code elements to map them to Style instances
        foreach (var tokenType in TokenTypes)
        {
            string scopeName = tokenType.Key.Humanize().Transform(To.TitleCase);
            var codeElement = tokenType.Value;

            var style = new Style(scopeName)
            {
                Foreground = codeElement.Foreground,
                Background = codeElement.Background,
                ReferenceName = codeElement.ReferenceName,
                Bold = codeElement.Bold,
                Italic = codeElement.Italic,
            };

            styleDictionary.Add(style);
        }

        return styleDictionary;
    }
}

public class TokenType
{
    [JsonPropertyName("foreground")]
    public string Foreground { get; set; } = default!;

    [JsonPropertyName("background")]
    public string Background { get; set; } = default!;

    [JsonPropertyName("reference_name")]
    public string ReferenceName { get; set; } = default!;

    [JsonPropertyName("bold")]
    public bool Bold { get; set; }

    [JsonPropertyName("italic")]
    public bool Italic { get; set; }
}
