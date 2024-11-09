using System.Reflection;
using System.Text.Json;
using BuildingBlocks.Serialization;
using BuildingBlocks.Utils;
using ColorCode;
using ColorCode.Common;
using ColorCode.Styling;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using HtmlHelper = BuildingBlocks.Utils.HtmlHelper;

namespace BuildingBlocks.MarkdigMarkdown;

public class ColorCodeBlockExtension(string theme) : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.BlockParsers.Contains<ColorCodeFenceBlockParser>())
            pipeline.BlockParsers.Insert(0, new ColorCodeFenceBlockParser(theme));
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}

public class ColorCodeFenceBlockParser : FencedCodeBlockParser
{
    private readonly HtmlFormatter _formatter;
    private readonly CodeSnippetTheme? _codeBlockTheme;

    public ColorCodeFenceBlockParser(string? theme)
    {
        var jsonTheme = FilesUtilities.ReadEmbeddedResource(
            Assembly.GetExecutingAssembly(),
            $"{nameof(BuildingBlocks)}.{nameof(MarkdigMarkdown)}.Themes.{theme ?? "vscode_light"}.json"
        );

        _codeBlockTheme = JsonSerializer.Deserialize<CodeSnippetTheme>(
            jsonTheme,
            JsonObjectSerializer.SnakeCaseOptions
        );
        var codeBlockStyleDictionary = _codeBlockTheme?.ToColorCodeStyleDictionary() ?? StyleDictionary.DefaultLight;
        _formatter = new HtmlFormatter(codeBlockStyleDictionary);
    }

    public override bool Close(BlockProcessor processor, Block block)
    {
        if (block is FencedCodeBlock fencedCodeBlock)
        {
            var background = _codeBlockTheme?.Background;
            if (!string.IsNullOrEmpty(background))
            {
                block.SetData("background", background);
            }

            //  Get the language and code content
            var language = fencedCodeBlock.Info?.Trim() ?? "md"; // e.g., "csharp"
            var code = fencedCodeBlock.Lines.ToString();

            // Apply syntax highlighting
            try
            {
                if (string.IsNullOrEmpty(code))
                    fencedCodeBlock.Lines = new StringLineGroup();

                var highlightedCode = _formatter.GetHtmlString(code, Languages.FindById(language));

                var codeBBCodeFormat = HtmlHelper.ConvertHtmlToBBCode(highlightedCode);

                fencedCodeBlock.Lines = new StringLineGroup(codeBBCodeFormat); //overide with new highlighted code
            }
            catch
            {
                // ignored
            }
        }

        return base.Close(processor, block);
    }
}
