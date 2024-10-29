using BuildingBlocks.Markdig;
using Spectre.Console.Rendering;

namespace BuildingBlocks.SpectreConsole.Markdown;

public class SpectreMarkdown(string markdownText) : Renderable
{
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var spectreResult = new List<Segment>();

        var markdownParser = new MarkdownParser();
        var markdownDocument = markdownParser.ToMarkdownDocument(markdownText);

        var markdownBlockRendering = new SpectreMarkdownBlockRendering(new SpectreMarkdownInlineRendering());

        //var markdownBlockRendering = new AnsiRenderer();

        foreach (var markdownBlock in markdownDocument)
        {
            spectreResult.AddRange(markdownBlockRendering.RenderBlock(markdownBlock).Render(options, maxWidth));
        }

        spectreResult.Add(Segment.LineBreak);

        return spectreResult;
    }
}
