namespace Clients.Options;

public class PolicyOptions
{
    public int RetryCount { get; set; } = 3;
    public int BreakDuration { get; set; } = 30;
    public int TimeoutSeconds { get; set; } = 420;
}
