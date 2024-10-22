using BuildingBlocks.LLM;

namespace Clients.Models;

public class Token(string value)
{
    public string Value { get; } = value;
    public int Count { get; } = TokenizerHelper.GPT4TokenCount(value);
    public DateTime CreatedAt { get; private set; } = DateTime.Now;

    public static Token operator +(Token left, Token right)
    {
        return new Token(left.Value + right.Value);
    }
}
