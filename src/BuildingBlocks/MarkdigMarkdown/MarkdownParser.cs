using Markdig;
using Markdig.Syntax;

namespace BuildingBlocks.MarkdigMarkdown;

public class MarkdownParser
{
    public MarkdownDocument ToMarkdownDocument(string markdown, string theme = "dracula")
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseSoftlineBreakAsHardlineBreak()
            .UseColorCodeBlock(theme)
            .Build();

        // parser Markdown text to markdig documents object
        var markdownDocument = Markdown.Parse(markdown, pipeline);

        return markdownDocument;
    }
}
