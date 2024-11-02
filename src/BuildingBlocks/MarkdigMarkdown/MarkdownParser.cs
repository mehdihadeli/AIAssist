using Markdig;
using Markdig.Syntax;

namespace BuildingBlocks.MarkdigMarkdown;

public class MarkdownParser
{
    public MarkdownDocument ToMarkdownDocument(string markdown, string theme = "dracula")
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseColorCodeBlock(theme).Build();

        // parser Markdown text to markdig documents object
        var markdownDocument = global::Markdig.Markdown.Parse(markdown, pipeline);

        return markdownDocument;
    }
}
