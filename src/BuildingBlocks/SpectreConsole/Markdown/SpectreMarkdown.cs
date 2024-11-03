using BuildingBlocks.MarkdigMarkdown;
using Spectre.Console.Rendering;

namespace BuildingBlocks.SpectreConsole.Markdown;

public class SpectreMarkdown(string markdownText, string? theme = "dracula") : Renderable
{
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var spectreResult = new List<Segment>();

        var themeObject = ThemeLoader.LoadTheme(theme);

        var markdownParser = new MarkdownParser();
        var markdigMarkdownDocument = markdownParser.ToMarkdownDocument(
            markdownText,
            theme: themeObject?.CodeBlockStyle.ThemeName ?? "vscode_light"
        );

        foreach (var markdigMarkdownBlock in markdigMarkdownDocument)
        {
            using var spectreMarkdownBlockRendering = new SpectreMarkdownBlockRendering(themeObject!);
            var blockRenderable = spectreMarkdownBlockRendering.RenderBlock(markdigMarkdownBlock);
            var segments = blockRenderable.Render(options, maxWidth);
            spectreResult.AddRange(segments);
        }

        spectreResult.Add(Segment.LineBreak);

        return spectreResult;
    }
}
