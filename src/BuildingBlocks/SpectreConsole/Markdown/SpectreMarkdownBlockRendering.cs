using BuildingBlocks.Markdig;
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

internal class SpectreMarkdownBlockRendering(SpectreMarkdownInlineRendering inlineRendering)
{
    public IRenderable RenderBlock(Block markdigBlock, Justify alignment = Justify.Left, bool suppressNewLine = false)
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
                return RenderFencedCodeBlock(fencedCodeBlock);
            case CodeBlock codeBlock:
                return RenderCodeBlock(codeBlock);
            case ListBlock listBlock:
                result = RenderListBlock(listBlock);
                break;
            case ListItemBlock listItemBlock:
                result = RenderListItemBlock(listItemBlock);
                break;
            case QuoteBlock quoteBlock:
                result = RenderQuoteBlock(quoteBlock);
                break;
            case HeadingBlock headingBlock:
                result = RenderHeadingBlock(headingBlock);
                break;
            case ParagraphBlock paragraphBlock:
                if (suppressNewLine)
                    return RenderParagraphBlock(paragraphBlock, alignment, suppressNewLine);
                result = RenderParagraphBlock(paragraphBlock, alignment, suppressNewLine);
                break;
            case ThematicBreakBlock:
                result = new Rule { Style = new Style(decoration: Decoration.Bold), Border = BoxBorder.Double };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(markdigBlock));
        }

        if (result is not null)
        {
            return new SpectreCompositeRenderable(new List<IRenderable> { result, new Text(Environment.NewLine) });
        }

        return Text.Empty;
    }

    private static IRenderable RenderCodeBlock(CodeBlock codeBlock)
    {
        var blockContents = codeBlock.Lines.ToString();
        return new Panel(blockContents)
        {
            Header = new PanelHeader("code"),
            Expand = true,
            Border = BoxBorder.Rounded,
        };
    }

    private static IRenderable RenderFencedCodeBlock(FencedCodeBlock fencedCodeBlock)
    {
        var bbcode = fencedCodeBlock.Lines.ToString();
        var backgroundColor = fencedCodeBlock.GetData("background") as string;

        return string.IsNullOrEmpty(backgroundColor)
            ? new CustomPanel(bbcode)
            {
                Expand = true,
                Border = BoxBorder.Rounded,
                Header = new PanelHeader(fencedCodeBlock.Info ?? "code"),
            }
            : new CustomPanel(bbcode, Style.Parse($"on {backgroundColor}"))
            {
                Expand = true,
                Border = BoxBorder.Rounded,
                Header = new PanelHeader(fencedCodeBlock.Info ?? "code"),
            };
    }

    private IRenderable RenderQuoteBlock(QuoteBlock quoteBlock)
    {
        foreach (var subBlock in quoteBlock)
        {
            if (subBlock is ParagraphBlock paragraph)
            {
                return new SpectreCompositeRenderable(
                    new List<IRenderable>
                    {
                        new Markup($"[deepskyblue1] {CharacterSet.QuotePrefix} [/]"),
                        RenderParagraphBlock(paragraph, Justify.Left),
                    }
                );
            }
        }

        return new Text("");
    }

    private IRenderable RenderListItemBlock(ListItemBlock listItemBlock)
    {
        return new SpectreCompositeRenderable(listItemBlock.Select(x => RenderBlock(x, suppressNewLine: true)));
    }

    private IRenderable RenderTableBlock(Table table)
    {
        if (table.IsValid())
        {
            var renderedTable = new Spectre.Console.Table();

            // Safe to unconditionally cast to TableRow as IsValid() ensures this is the case under the hood
            foreach (var tableRow in table.Cast<TableRow>())
                if (tableRow.IsHeader)
                    AddColumnsToTable(tableRow, table.ColumnDefinitions, renderedTable);
                else
                    AddRowToTable(tableRow, table.ColumnDefinitions, renderedTable);

            return renderedTable;
        }

        return new Text("Invalid table structure", new Style(Color.Red));
    }

    private void AddColumnsToTable(
        TableRow tableRow,
        List<TableColumnDefinition> columnDefinitions,
        Spectre.Console.Table renderedTable
    )
    {
        // Safe to unconditionally cast to TableCell as IsValid() ensures this is the case under the hood
        foreach (var (cell, def) in tableRow.Cast<TableCell>().Zip(columnDefinitions))
            renderedTable.AddColumn(new TableColumn(RenderTableCell(cell, def.Alignment)));
    }

    private void AddRowToTable(
        TableRow tableRow,
        List<TableColumnDefinition> columnDefinitions,
        Spectre.Console.Table renderedTable
    )
    {
        var renderedRow = new List<IRenderable>();

        // Safe to unconditionally cast to TableCell as IsValid() ensures this is the case under the hood
        foreach (var (cell, def) in tableRow.Cast<TableCell>().Zip(columnDefinitions))
            renderedRow.Add(RenderTableCell(cell, def.Alignment));

        renderedTable.AddRow(renderedRow);
    }

    private IRenderable RenderTableCell(TableCell tableCell, TableColumnAlign? markdownAlignment)
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
            tableCell.Select(x => RenderBlock(x, consoleAlignment, suppressNewLine: true))
        );
    }

    private IRenderable RenderListBlock(ListBlock listBlock)
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
            itemPrefixes = Enumerable.Repeat(CharacterSet.ListBullet, listBlock.Count);
        }

        var paddedItemPrefixes = itemPrefixes.Select(x => new Text($"  {x} "));

        return new SpectreCompositeRenderable(
            [.. Interleave(paddedItemPrefixes, listBlock.Select(x => RenderBlock(x)))]
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

    private IRenderable RenderParagraphBlock(
        ParagraphBlock paragraphBlock,
        Justify alignment,
        bool suppressNewLine = false
    )
    {
        var text = inlineRendering.RenderContainerInline(paragraphBlock.Inline, alignment: alignment);

        if (!suppressNewLine)
        {
            return new SpectreCompositeRenderable(new List<IRenderable> { text, new Text(Environment.NewLine) });
        }

        return new SpectreCompositeRenderable(new List<IRenderable> { text });
    }

    private IRenderable RenderHeadingBlock(HeadingBlock headingBlock)
    {
        var headingText = headingBlock.Inline?.GetInlineContent() ?? string.Empty;

        if (headingBlock.Level == 1)
        {
            return new SpectreCompositeRenderable(
                new List<IRenderable>
                {
                    new Text($"# {headingText}", style: Style.Parse("white on slateblue1 bold")),
                    new Text(Environment.NewLine),
                }
            );
        }

        string prefix = new string('#', headingBlock.Level);

        return new SpectreCompositeRenderable(
            new List<IRenderable>
            {
                new Markup($"[deepskyblue1 bold]{prefix} {headingText}[/]"),
                new Text(Environment.NewLine),
            }
        );
    }
}
