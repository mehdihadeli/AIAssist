using BuildingBlocks.SpectreConsole.Themes;
using Markdig;
using Markdig.Syntax;

namespace BuildingBlocks.Markdig;

public class MarkdownParser
{
    public MarkdownDocument ToMarkdownDocument(string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseColorCodeBlock(DraculaTheme.DraculaDark)
            .Build();

        // parser Markdown text to markdig documents object
        var markdownDocument = Markdown.Parse(markdown, pipeline);

        return markdownDocument;
    }
}
