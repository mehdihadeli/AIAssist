using ColorCode;
using ColorCode.Common;
using ColorCode.Styling;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;

namespace BuildingBlocks.Markdig;

public class ColorCodeBlockExtension(StyleDictionary theme) : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.BlockParsers.Contains<ColorCodeFenceBlockParser>())
        {
            pipeline.BlockParsers.Insert(0, new ColorCodeFenceBlockParser(theme));
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}

public class ColorCodeFenceBlockParser(StyleDictionary theme) : FencedCodeBlockParser
{
    private readonly HtmlFormatter _formatter = new(Style: theme);

    public override bool Close(BlockProcessor processor, Block block)
    {
        if (block is FencedCodeBlock fencedCodeBlock)
        {
            var plainText = theme[ScopeName.PlainText];
            var background = plainText.Background;

            block.SetData("background", background);

            //  Get the language and code content
            var language = fencedCodeBlock.Info.Trim(); // e.g., "csharp"
            var code = fencedCodeBlock.Lines.ToString();

            // Apply syntax highlighting
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    fencedCodeBlock.Lines = new StringLineGroup();
                }

                var highlightedCode = _formatter.GetHtmlString(code, Languages.FindById(language));
                var codeBBCodeFormat = HtmlHelper.ConvertHtmlToBBCode(highlightedCode);
                fencedCodeBlock.Lines = new StringLineGroup(codeBBCodeFormat); //override with new highlighted code
            }
            catch
            {
                // ignored
            }
        }

        return base.Close(processor, block);
    }
}
