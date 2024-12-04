using System.Text;
using BuildingBlocks.SpectreConsole.Markdown;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BuildingBlocks.SpectreConsole;

public class StreamPrinter(IAnsiConsole console, bool useMarkdown)
{
    private readonly StringBuilder _completeResponse = new();

    public async Task<string> PrintAsync(
        IAsyncEnumerable<string?> textStream,
        CancellationToken cancellationToken = default
    )
    {
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
            .Live(new Panel(new Markup(string.Empty)) { Expand = true, Border = BoxBorder.None })
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .StartAsync(async ctx =>
            {
                if (firstStream is not null)
                {
                    await UpdateLiveDisplay(firstStream, ctx);

                    while (await enumerator.MoveNextAsync())
                    {
                        var text = enumerator.Current;

                        if (!string.IsNullOrEmpty(text))
                        {
                            await UpdateLiveDisplay(text, ctx);
                        }
                    }
                }
            });

        return _completeResponse.ToString();
    }

    private Task UpdateLiveDisplay(string? text, LiveDisplayContext ctx)
    {
        if (text is null)
            return Task.CompletedTask;

        _completeResponse.Append(text);

        var printContent = Print();

        ctx.UpdateTarget(printContent);

        ctx.Refresh();

        return Task.CompletedTask;
    }

    private IRenderable Print()
    {
        // Choose between Markdown or standard rendering based on `useMarkdown`
        if (!useMarkdown)
        {
            return new Panel(new Markup(_completeResponse.ToString())) { Expand = true, Border = BoxBorder.None };
        }

        return new Panel(new SpectreMarkdown(_completeResponse.ToString())) { Expand = true, Border = BoxBorder.None };
    }
}
