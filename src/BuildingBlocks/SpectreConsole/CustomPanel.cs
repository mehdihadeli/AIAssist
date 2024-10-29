using Spectre.Console;
using Spectre.Console.Rendering;

namespace BuildingBlocks.SpectreConsole;

public sealed class CustomPanel : Renderable
{
    private readonly Panel _internalPanel;

    public CustomPanel(IRenderable content, Style? style = null)
    {
        _internalPanel = new Panel(content);
        Style = style;
    }

    public CustomPanel(string content, Style? style = null)
    {
        _internalPanel = new Panel(content);
        Style = style;
    }

    public Style? Style { get; set; }

    public BoxBorder Border
    {
        get => _internalPanel.Border;
        set => _internalPanel.Border = value;
    }

    public Style? BorderStyle
    {
        get => _internalPanel.BorderStyle;
        set => _internalPanel.BorderStyle = value;
    }

    public bool Expand
    {
        get => _internalPanel.Expand;
        set => _internalPanel.Expand = value;
    }

    public Padding? Padding
    {
        get => _internalPanel.Padding;
        set => _internalPanel.Padding = value;
    }

    public PanelHeader? Header
    {
        get => _internalPanel.Header;
        set => _internalPanel.Header = value;
    }

    public int? Width
    {
        get => _internalPanel.Width;
        set => _internalPanel.Width = value;
    }

    public int? Height
    {
        get => _internalPanel.Height;
        set => _internalPanel.Height = value;
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        // Render the internal panel to segments
        var panelSegments = ((IRenderable)_internalPanel).Render(options, maxWidth);

        // Apply the CustomPanel's style to each segment
        foreach (var segment in panelSegments)
        {
            if (Style is not null)
                yield return new Segment(segment.Text, segment.Style.Combine(Style));
            else
            {
                yield return new Segment(segment.Text);
            }
        }
    }
}
