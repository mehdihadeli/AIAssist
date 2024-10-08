namespace BuildingBlocks.Cli;

public interface ICommandHandler<in TOptions>
{
    Task<int> HandleAsync(TOptions options, CancellationToken cancellationToken);
}
