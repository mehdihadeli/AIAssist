using System.Text;
using BuildingBlocks.SpectreConsole.Markdown;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BuildingBlocks.SpectreConsole;

public class StreamPrinter(IAnsiConsole console, bool useMarkdown)
{
    private readonly StringBuilder _currentText = new();

    public async Task<string> PrintAsync(
        IAsyncEnumerable<string?> textStream,
        CancellationToken cancellationToken = default
    )
    {
        var initialContent = new Panel(new Markup(string.Empty))
        {
            Border = BoxBorder.Rounded,
            Header = new PanelHeader("Loading", Justify.Center),
        };

        var completeResponse = new StringBuilder();

        var enumerator = textStream.GetAsyncEnumerator(cancellationToken);
        string? firstStream = string.Empty;

        await console
            .Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("deepskyblue1 bold"))
            .StartAsync(
                "processing response...",
                async statusCtx =>
                {
                    if (await enumerator.MoveNextAsync())
                    {
                        firstStream = enumerator.Current;
                    }
                }
            );

        // Start the live display for console
        await console
            .Live(initialContent)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                if (!string.IsNullOrEmpty(firstStream))
                {
                    UpdateLiveDisplay(firstStream, completeResponse, ctx);

                    while (await enumerator.MoveNextAsync())
                    {
                        var text = enumerator.Current;

                        UpdateLiveDisplay(text, completeResponse, ctx);

                        await Task.Delay(50, cancellationToken);
                    }
                }
            });

        return completeResponse.ToString();
    }

    private void UpdateLiveDisplay(string? text, StringBuilder completeResponse, LiveDisplayContext ctx)
    {
        // Generate the print content
        completeResponse.Append(text);

        var printContent = Print(text);
        // Update the live print with the current content
        ctx.UpdateTarget(printContent);

        // Refresh the context to render the latest updates
        ctx.Refresh();
    }

    private IRenderable Print(string? text)
    {
        _currentText.Append(text);

        if (!useMarkdown)
        {
            return new Markup(_currentText.ToString());
        }

        return new SpectreMarkdown(_currentText.ToString());
    }
}
