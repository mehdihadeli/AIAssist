using System.Text;
using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.Emoji;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.SmartyPants;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax.Inlines;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BuildingBlocks.SpectreConsole.Markdown;

internal sealed class SpectreMarkdownInlineRendering
{
    private IRenderable RenderInline(Inline inline, Style style, Justify alignment)
    {
        switch (inline)
        {
            case SmartyPant smartyPant:
            case MathInline mathInline:
            case HtmlEntityInline htmlEntityInline:
            case HtmlInline htmlInline:
            case FootnoteLink footnoteLink:
            case CustomContainerInline customContainerInline:
            case AbbreviationInline abbreviationInline:
            case AutolinkInline autolinkInline:
                break;

            case EmojiInline emojiInline:
                return new Text(Emoji.Replace(emojiInline.Content.ToString()), style) { Justification = alignment };
            case CodeInline codeInline:
                return WriteCodeInline(codeInline);

            case EmphasisInline emphasisInline:
                var styleDecoration =
                    emphasisInline.DelimiterChar == '~'
                        ? Decoration.Strikethrough
                        : emphasisInline.DelimiterCount switch
                        {
                            1 => Decoration.Italic,
                            2 => Decoration.Bold,
                            _ => Decoration.None,
                        };
                var emphasisChildStyle = new Style(decoration: styleDecoration);
                return RenderContainerInline(emphasisInline, emphasisChildStyle);
            case LinkInline linkInline:
                var linkInlineChildStyle = new Style(link: linkInline.Url);
                return RenderContainerInline(linkInline, linkInlineChildStyle);

            // We don't care what delimiters were used to compose a particular document structure
            case PipeTableDelimiterInline:
                break;
            case EmphasisDelimiterInline:
                break;
            case LinkDelimiterInline:
                break;
            case LineBreakInline:
                return new Text("\n");
            case ContainerInline containerInline:
                return RenderContainerInline(containerInline);
            case LiteralInline literalInline:
                return new Markup(literalInline.Content.ToString().EscapeMarkup(), style) { Justification = alignment };
            case TaskList task:
                var bullet = task.Checked ? CharacterSet.TaskListBulletDone : CharacterSet.TaskListBulletToDo;
                return new Markup($"[deepskyblue1]{bullet.EscapeMarkup()}[/]");
            default:
                throw new ArgumentOutOfRangeException(nameof(inline));
        }

        return Text.Empty;
    }

    public IRenderable RenderContainerInline(
        ContainerInline inline,
        Style? style = null,
        Justify alignment = Justify.Left
    )
    {
        return new SpectreCompositeRenderable(inline.Select(x => RenderInline(x, style ?? Style.Plain, alignment)));
    }

    private Markup WriteCodeInline(CodeInline code)
    {
        var sb = new StringBuilder();

        sb.Append(CharacterSet.InlineCodeOpening);
        sb.Append(code.Content.EscapeMarkup());
        sb.Append(CharacterSet.InlineCodeClosing);

        return new Markup(
            sb.ToString(),
            new Style(foreground: Color.White, background: Color.SlateBlue1, decoration: Decoration.Bold)
        );
    }
}
