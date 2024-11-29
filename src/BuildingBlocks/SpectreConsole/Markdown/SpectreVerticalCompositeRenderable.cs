using Spectre.Console;
using Spectre.Console.Rendering;

namespace BuildingBlocks.SpectreConsole.Markdown;

internal class SpectreVerticalCompositeRenderable(IEnumerable<IRenderable> renderables) : Renderable
{
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        return renderables.SelectMany(x => x.Render(options, maxWidth));
    }
}

//  Stacking elements vertically or adding multiple horizontal rows without extra space
internal class SpectreHorizontalCompositeRenderable(IEnumerable<IRenderable> renderablesElements) : Renderable
{
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        // Use Rows to combine renderables items in multiple horizontal rows without extra space
        IRenderable rows = new Rows(renderablesElements);
        return rows.Render(options, maxWidth);
    }
}