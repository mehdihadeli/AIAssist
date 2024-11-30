using System.Text;
using BuildingBlocks.SpectreConsole.StyleElements;
using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.Emoji;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.JiraLinks;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.SmartyPants;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax.Inlines;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BuildingBlocks.SpectreConsole.Markdown;

internal sealed class SpectreMarkdownInlineRendering(ColorTheme colorTheme)
{
    private IRenderable RenderInline(Inline inline, Style style, Justify alignment)
    {
        switch (inline)
        {
            case JiraLink jiraLink:
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
                var codeStyle = CreateStyle(colorTheme.CodeStyle);
                return WriteCodeInline(codeInline, codeStyle);

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
                var emphasisChildStyle = CreateStyle(colorTheme.EmphStyle)
                    .Combine(new Style(decoration: styleDecoration));
                return RenderContainerInline(emphasisInline, emphasisChildStyle);
            case LinkInline linkInline:
                string underline = colorTheme.LinkStyle.Underline ? "underline" : "default";
                return new Markup(
                    $"[{colorTheme.LinkStyle.LinkTextForeground}]{linkInline.FirstChild}[/] [{colorTheme.LinkStyle.LinkForeground} {"link"} {underline}]{linkInline.Url}[/]"
                );

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
                return new Markup(literalInline.Content.ToString(), style) { Justification = alignment };
            case TaskList task:
                var taskStyle = CreateStyle(colorTheme.TaskStyle);
                var bullet = task.Checked
                    ? colorTheme.TaskStyle.Ticked ?? CharacterSet.TaskListBulletDone
                    : colorTheme.TaskStyle.UnTicked ?? CharacterSet.TaskListBulletToDo;
                return new Markup($"[{colorTheme.TaskStyle.PrefixForeground}]{bullet.EscapeMarkup()}[/]", taskStyle);
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
        return new SpectreVerticalCompositeRenderable(
            inline.Select(x => RenderInline(x, style ?? Style.Plain, alignment))
        );
    }

    private Markup WriteCodeInline(CodeInline code, Style style)
    {
        var sb = new StringBuilder();

        sb.Append(CharacterSet.InlineCodeOpening);
        sb.Append(code.Content.EscapeMarkup());
        sb.Append(CharacterSet.InlineCodeClosing);

        return new Markup($" {sb} ", style);
    }

    private Style CreateStyle(StyleBase styleBase, Style? style = null)
    {
        style ??= Style.Parse(CreateStringStyle(styleBase));

        return style;
    }

    private string CreateStringStyle(StyleBase styleBase)
    {
        var italic = styleBase.Italic ? "italic" : "default";
        var bold = styleBase.Bold ? "bold" : "default";
        var underline = styleBase.Underline ? "underline" : "default";

        var style =
            $"{
                styleBase.Foreground ?? colorTheme.Foreground ?? "default"
            } on {
                styleBase.Background ?? "default"
            } {
                italic
            } {
                bold
            } {
                underline
            }";

        return style;
    }
}
