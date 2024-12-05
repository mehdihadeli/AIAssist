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
        return markdigBlock switch
        {
            BlankLineBlock => new Text(Environment.NewLine),
            EmptyBlock => new Text(string.Empty),
            Table table => AppendBreakAfter(RenderTableBlock(table, style)),
            FencedCodeBlock fencedCodeBlock => AppendBreakAfter(RenderFenceBlock(fencedCodeBlock)),
            CodeBlock codeBlock => AppendBreakAfter(RenderCodeBlock(codeBlock)),
            ListBlock listBlock => AppendBreakBeforeAfter(
                RenderListBlock(listBlock, CreateStyle(_colorTheme.ListStyle, style))
            ),
            ListItemBlock listItemBlock => RenderListItemBlock(listItemBlock, style),
            QuoteBlock quoteBlock => AppendBreakAfter(
                RenderQuoteBlock(quoteBlock, CreateStyle(_colorTheme.BlockQuoteStyle, style))
            ),
            HeadingBlock headingBlock => AppendBreakAfter(
                RenderHeadingBlock(headingBlock, CreateStyle(_colorTheme.HeadStyle, style))
            ),
            ParagraphBlock paragraphBlock => RenderParagraphBlock(
                paragraphBlock,
                alignment,
                CreateStyle(_colorTheme.ParagraphStyle, style),
                suppressNewLine
            ),
            ThematicBreakBlock thematicBreakBlock => new Text(thematicBreakBlock.Content.Text),
            _ => Text.Empty,
        };
    }

    private IRenderable AppendBreakAfter(IRenderable renderable)
    {
        //  // or using SpectreHorizontalCompositeRenderable
        // return new SpectreHorizontalCompositeRenderable(new List<IRenderable> { renderable, new Text(string.Empty) });

        return new SpectreVerticalCompositeRenderable(
            //  break current line with `\n` and create a empty line with `NewLine`
            new List<IRenderable> { renderable, new Text("\n"), new Text(Environment.NewLine) }
        );
    }

    private IRenderable AppendBreakBefore(IRenderable renderable)
    {
        return new SpectreVerticalCompositeRenderable(
            new List<IRenderable> { new Text(Environment.NewLine), renderable }
        );
    }

    private IRenderable AppendBreakBeforeAfter(IRenderable renderable)
    {
        return new SpectreVerticalCompositeRenderable(
            new List<IRenderable>
            {
                new Text(Environment.NewLine),
                renderable,
                new Text("\n"),
                new Text(Environment.NewLine),
            }
        );
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

    private IRenderable RenderCodeBlock(CodeBlock codeBlock)
    {
        var blockContents = codeBlock.Lines.ToString();
        return new Panel(blockContents)
        {
            Header = new PanelHeader("code"),
            Expand = true,
            Border = BoxBorder.Rounded,
        };
    }

    private IRenderable RenderQuoteBlock(QuoteBlock quoteBlock, Style style)
    {
        foreach (var subBlock in quoteBlock)
            if (subBlock is ParagraphBlock paragraph)
                return new SpectreVerticalCompositeRenderable(
                    new List<IRenderable>
                    {
                        new Markup(
                            $"{CharacterSet.QuotePrefix} ",
                            Style.Parse(_colorTheme.BlockQuoteStyle.PrefixForeground)
                        ),
                        RenderParagraphBlock(paragraph, Justify.Left, style: style),
                    }
                );

        return Text.Empty;
    }

    private IRenderable RenderListBlock(ListBlock listBlock, Style style, int indentLevel = 0)
    {
        int startNumber = int.TryParse(listBlock.OrderedStart, out var parsedNumber) ? parsedNumber : 1;

        // Generate item prefixes
        var itemPrefixes = listBlock.IsOrdered
            ? Enumerable.Range(startNumber, listBlock.Count).Select(num => $"{num}{listBlock.OrderedDelimiter}")
            : Enumerable.Repeat(_colorTheme.ListStyle.BlockPrefix ?? CharacterSet.ListBullet, listBlock.Count);

        var renderedItems = listBlock
            .OfType<ListItemBlock>()
            .Select(
                (itemBlock, index) =>
                {
                    // Generate the prefix for the current list item
                    var prefix = new Text(
                        $"{new string(' ', indentLevel * 4)}{itemPrefixes.ElementAt(index)} ",
                        style: Style.Parse(_colorTheme.ListStyle.PrefixForeground ?? "default")
                    );

                    var itemContent = RenderListItemBlock(itemBlock, style, indentLevel);

                    // Combine the prefix and content
                    return new SpectreVerticalCompositeRenderable([prefix, itemContent]);
                }
            );

        // Combine all rendered items without introducing extra line break
        return new SpectreHorizontalCompositeRenderable(renderedItems);
    }

    private IRenderable RenderListItemBlock(ListItemBlock listItemBlock, Style? style = null, int indentLevel = 0)
    {
        // Render children blocks for the list item
        var renderedChildren = listItemBlock.Select(child =>
        {
            if (child is ListBlock nestedList)
            {
                // Indent nested lists
                return RenderListBlock(nestedList, style ?? Style.Plain, indentLevel + 1);
            }

            // Maintain indentation for other blocks
            return RenderBlock(child, suppressNewLine: true, style: style);
        });

        // Combine all child blocks without unnecessary line break
        return new SpectreHorizontalCompositeRenderable(renderedChildren);
    }

    private IRenderable RenderTableBlock(Table table, Style? style = null)
    {
        if (table.IsValid())
        {
            var renderedTable = new Spectre.Console.Table();

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
        var renderedRow = tableRow
            .Cast<TableCell>()
            .Zip(columnDefinitions)
            .Select(cellDef => RenderTableCell(cellDef.First, cellDef.Second.Alignment, style))
            .ToList();

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
            _ => throw new ArgumentOutOfRangeException(),
        };

        return new SpectreVerticalCompositeRenderable(
            tableCell.Select(x => RenderBlock(x, consoleAlignment, style, true))
        );
    }

    private IRenderable RenderHeadingBlock(HeadingBlock headingBlock, Style style)
    {
        var headingText = headingBlock.Inline?.GetInlineContent() ?? string.Empty;
        var prefix = new string('#', headingBlock.Level);

        return new Markup($" {prefix} {headingText} ", style);
    }

    private IRenderable RenderParagraphBlock(
        ParagraphBlock paragraphBlock,
        Justify alignment,
        Style style,
        bool suppressNewLine = false
    )
    {
        var text = _inlineRendering.RenderContainerInline(paragraphBlock.Inline, style, alignment: alignment);

        return suppressNewLine ? text : AppendBreakAfter(text);
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

        return $"{styleBase.Foreground ?? _colorTheme.Foreground ?? "default"} on {styleBase.Background ?? "default"} {italic} {bold} {underline}";
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
