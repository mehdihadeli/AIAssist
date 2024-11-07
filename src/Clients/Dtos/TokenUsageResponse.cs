namespace Clients.Dtos;

public record TokenUsageResponse(
    int InputTokens,
    decimal InputCostPerToken,
    int OutputTokens,
    decimal OutputCostPerToken
)
{
    public int TotalTokens => InputTokens + OutputTokens;
}
