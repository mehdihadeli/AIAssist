using Spectre.Console.Rendering;

namespace BuildingBlocks.SpectreConsole.Markdown;

internal class SpectreVerticalCompositeRenderable(IEnumerable<IRenderable> renderables) : Renderable
{
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        return renderables.SelectMany(x => x.Render(options, maxWidth));
    }
}
