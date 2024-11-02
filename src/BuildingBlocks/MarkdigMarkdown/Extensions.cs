using Markdig;
using Markdig.Syntax.Inlines;

namespace BuildingBlocks.MarkdigMarkdown;

public static class Extensions
{
    public static MarkdownPipelineBuilder UseColorCodeBlock(
        this MarkdownPipelineBuilder pipeline,
        string theme = "dracula"
    )
    {
        pipeline.Extensions.AddIfNotAlready(new ColorCodeBlockExtension(theme));

        return pipeline;
    }

    public static string GetInlineContent(this ContainerInline containerInline)
    {
        return string.Join(Environment.NewLine, containerInline.Select(inline => inline.ToString()));
    }
}
