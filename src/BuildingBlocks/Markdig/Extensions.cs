using ColorCode.Styling;
using Markdig;
using Markdig.Syntax.Inlines;

namespace BuildingBlocks.Markdig;

public static class Extensions
{
    public static MarkdownPipelineBuilder UseColorCodeBlock(
        this MarkdownPipelineBuilder pipeline,
        StyleDictionary theme
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
