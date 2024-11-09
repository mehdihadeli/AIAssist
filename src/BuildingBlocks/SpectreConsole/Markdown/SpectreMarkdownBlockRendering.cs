using BuildingBlocks.MarkdigMarkdown;
using BuildingBlocks.SpectreConsole.StyleElements;
using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.DefinitionLists;
using Markdig.Extensions.Figures;
using Markdig.Extensions.Footers;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Spectre.Console;
using Spectre.Console.Rendering;
using Table = Markdig.Extensions.Tables.Table;
using TableRow = Markdig.Extensions.Tables.TableRow;

namespace BuildingBlocks.SpectreConsole.Markdown;

internal class SpectreMarkdownBlockRendering : IDisposable
{
    private readonly ColorTheme _colorTheme;
    private readonly SpectreMarkdownInlineRendering _inlineRendering;

    public SpectreMarkdownBlockRendering(ColorTheme colorTheme)
    {
        _colorTheme = colorTheme;
        _inlineRendering = new SpectreMarkdownInlineRendering(_colorTheme);
    }

    public IRenderable RenderBlock(
        Block markdigBlock,
        Justify alignment = Justify.Left,
        Style? style = null,
        bool suppressNewLine = false
    )
    {
        IRenderable? result = null;
        switch (markdigBlock)
        {
            case CustomContainer:
            case MathBlock:
            case Footnote:
            case FootnoteGroup:
            case FootnoteLinkReferenceDefinition:
            case Figure:
            case FigureCaption:
            case YamlFrontMatterBlock:
            case Abbreviation:
            case FooterBlock:
            case HtmlBlock:
            case DefinitionItem:
            case DefinitionList:
            case DefinitionTerm:
                break;

            // No point rendering these as the definitions are already reconciled by the parser.
            case HeadingLinkReferenceDefinition:
            case LinkReferenceDefinitionGroup:
            case LinkReferenceDefinition:
                break;

            case BlankLineBlock:
                return new Text(Environment.NewLine);
            case EmptyBlock:
                return new Text(Environment.NewLine);
            case Table table:
                result = RenderTableBlock(table);
                break;
            case FencedCodeBlock fencedCodeBlock:
                return RenderFenceBlock(fencedCodeBlock);
            case CodeBlock codeBlock:
                var blockContents = codeBlock.Lines.ToString();
                result = new Panel(blockContents)
                {
                    Header = new PanelHeader("code"),
                    Expand = true,
                    Border = BoxBorder.Rounded,
                };
                break;
            case ListBlock listBlock:
                result = RenderListBlock(listBlock, CreateStyle(_colorTheme.ListStyle, style));
                break;
            case ListItemBlock listItemBlock:
                result = RenderListItemBlock(listItemBlock, style);
                break;
            case QuoteBlock quoteBlock:
                result = RenderQuoteBlock(quoteBlock, CreateStyle(_colorTheme.BlockQuoteStyle, style));
                break;
            case HeadingBlock headingBlock:
                result = RenderHeadingBlock(headingBlock, CreateStyle(_colorTheme.HeadStyle, style));
                break;
            case ParagraphBlock paragraphBlock:
                if (suppressNewLine)
                    return RenderParagraphBlock(
                        paragraphBlock,
                        alignment,
                        CreateStyle(_colorTheme.ParagraphStyle, style),
                        suppressNewLine
                    );
                result = RenderParagraphBlock(
                    paragraphBlock,
                    alignment,
                    CreateStyle(_colorTheme.ParagraphStyle, style),
                    suppressNewLine
                );
                break;
            case ThematicBreakBlock:
                result = new Rule { Style = new Style(decoration: Decoration.Bold), Border = BoxBorder.Double };
                break;
        }

        if (result is not null)
            return new SpectreCompositeRenderable(new List<IRenderable> { result, new Text(Environment.NewLine) });

        return Text.Empty;
    }

    private IRenderable RenderFenceBlock(FencedCodeBlock fencedCodeBlock)
    {
        var bbcode = fencedCodeBlock.Lines.ToString();
        var backgroundColor = fencedCodeBlock.GetData("background") ?? "default";

        return new Padder(
            new CustomPanel(bbcode, Style.Parse($"on {backgroundColor}"))
            {
                Expand = true,
                Border = BoxBorder.Rounded,
                Header = new PanelHeader(fencedCodeBlock.Info ?? "code"),
            }
        ).PadLeft(_colorTheme.CodeBlockStyle.Margin);
    }

    private IRenderable RenderQuoteBlock(QuoteBlock quoteBlock, Style style)
    {
        foreach (var subBlock in quoteBlock)
            if (subBlock is ParagraphBlock paragraph)
                return new SpectreCompositeRenderable(
                    new List<IRenderable>
                    {
                        new Markup(
                            $"{CharacterSet.QuotePrefix} ",
                            Style.Parse(_colorTheme.BlockQuoteStyle.PrefixForeground)
                        ),
                        RenderParagraphBlock(paragraph, Justify.Left, style: style),
                    }
                );

        return new Text("");
    }

    private IRenderable RenderListBlock(ListBlock listBlock, Style style)
    {
        IEnumerable<string>? itemPrefixes;
        if (listBlock.IsOrdered)
        {
            var startNum = int.Parse(listBlock.OrderedStart);
            var orderedDelimiter = listBlock.OrderedDelimiter;
            itemPrefixes = Enumerable.Range(startNum, listBlock.Count).Select(num => $"{num}{orderedDelimiter}");
        }
        else
        {
            itemPrefixes = Enumerable.Repeat(
                _colorTheme.ListStyle.BlockPrefix ?? CharacterSet.ListBullet,
                listBlock.Count
            );
        }

        var paddedItemPrefixes = itemPrefixes.Select(x => new Text(
            $"  {x} ",
            style: Style.Parse(_colorTheme.ListStyle.PrefixForeground ?? "default")
        ));

        return new SpectreCompositeRenderable(
            [.. Interleave(paddedItemPrefixes, listBlock.Select(x => RenderBlock(x, style: style)))]
        );
    }

    private IRenderable RenderListItemBlock(ListItemBlock listItemBlock, Style? style = null)
    {
        return new SpectreCompositeRenderable(
            listItemBlock.Select(x => RenderBlock(x, suppressNewLine: true, style: style))
        );
    }

    private IRenderable RenderTableBlock(Table table, Style? style = null)
    {
        if (table.IsValid())
        {
            var renderedTable = new Spectre.Console.Table();

            // Safe to unconditionally cast to TableRow as IsValid() ensures this is the case under the hood
            foreach (var tableRow in table.Cast<TableRow>())
                if (tableRow.IsHeader)
                    AddColumnsToTable(tableRow, table.ColumnDefinitions, renderedTable, style);
                else
                    AddRowToTable(tableRow, table.ColumnDefinitions, renderedTable, style);

            return renderedTable;
        }

        return new Text("Invalid table structure", new Style(Color.Red));
    }

    private void AddColumnsToTable(
        TableRow tableRow,
        List<TableColumnDefinition> columnDefinitions,
        Spectre.Console.Table renderedTable,
        Style? style = null
    )
    {
        // Safe to unconditionally cast to TableCell as IsValid() ensures this is the case under the hood
        foreach (var (cell, def) in tableRow.Cast<TableCell>().Zip(columnDefinitions))
            renderedTable.AddColumn(new TableColumn(RenderTableCell(cell, def.Alignment, style)));
    }

    private void AddRowToTable(
        TableRow tableRow,
        List<TableColumnDefinition> columnDefinitions,
        Spectre.Console.Table renderedTable,
        Style? style = null
    )
    {
        var renderedRow = new List<IRenderable>();

        // Safe to unconditionally cast to TableCell as IsValid() ensures this is the case under the hood
        foreach (var (cell, def) in tableRow.Cast<TableCell>().Zip(columnDefinitions))
            renderedRow.Add(RenderTableCell(cell, def.Alignment, style));

        renderedTable.AddRow(renderedRow);
    }

    private IRenderable RenderTableCell(TableCell tableCell, TableColumnAlign? markdownAlignment, Style? style = null)
    {
        var consoleAlignment = markdownAlignment switch
        {
            TableColumnAlign.Left => Justify.Left,
            TableColumnAlign.Center => Justify.Center,
            TableColumnAlign.Right => Justify.Right,
            null => Justify.Left,
            _ => throw new ArgumentOutOfRangeException(
                nameof(markdownAlignment),
                markdownAlignment,
                "Unable to convert between Markdig alignment and Spectre.Console alignment"
            ),
        };

        return new SpectreCompositeRenderable(
            tableCell.Select(x => RenderBlock(x, consoleAlignment, style: style, true))
        );
    }

    private IRenderable RenderParagraphBlock(
        ParagraphBlock paragraphBlock,
        Justify alignment,
        Style style,
        bool suppressNewLine = false
    )
    {
        var text = _inlineRendering.RenderContainerInline(paragraphBlock.Inline, style, alignment: alignment);

        if (!suppressNewLine)
        {
            return new SpectreCompositeRenderable(new List<IRenderable> { text, new Text(Environment.NewLine) });
        }

        return new SpectreCompositeRenderable(new List<IRenderable> { text });
    }

    private IRenderable RenderHeadingBlock(HeadingBlock headingBlock, Style style)
    {
        var headingText = headingBlock.Inline?.GetInlineContent() ?? string.Empty;

        var prefix = new string('#', headingBlock.Level);

        return new SpectreCompositeRenderable(
            new List<IRenderable> { new Markup($" {prefix} {headingText} ", style), new Text(Environment.NewLine) }
        );
    }

    // write items for each bullet
    private static IEnumerable<T> Interleave<T>(IEnumerable<T> seqA, IEnumerable<T> seqB)
    {
        using var enumeratorA = seqA.GetEnumerator();
        using var enumeratorB = seqB.GetEnumerator();

        while (enumeratorA.MoveNext())
        {
            yield return enumeratorA.Current;

            if (enumeratorB.MoveNext())
                yield return enumeratorB.Current;
        }

        while (enumeratorB.MoveNext())
            yield return enumeratorB.Current;
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
                styleBase.Foreground ?? "default"
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
