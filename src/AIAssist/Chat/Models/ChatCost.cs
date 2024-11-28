using BuildingBlocks.Utils;

namespace AIAssist.Chat.Models;

public record ChatCost(int InputTokenCount, decimal InputCostPerToken, int OutputTokenCount, decimal OutputCostPerToken)
{
    public decimal TotalCost => OutputTokensCost + InputTokensCost;
    public decimal OutputTokensCost => OutputCostPerToken * OutputTokenCount;
    public decimal InputTokensCost => InputCostPerToken * InputTokenCount;
    public int TotalTokens => InputTokenCount + OutputTokenCount;

    public override string ToString()
    {
        return $"Output Tokens: {
            OutputTokenCount.FormatCommas()
        } | Output Cost: ${
            OutputTokensCost.FormatCommas()
        } | Input Tokens: {
            InputTokenCount.FormatCommas()
        } | Input Cost: ${
            InputTokensCost.FormatCommas()
        } | Total Tokens: {
            TotalTokens.FormatCommas()
        } | Total Cost: ${
            TotalCost.FormatCommas()
        }";
    }
}
