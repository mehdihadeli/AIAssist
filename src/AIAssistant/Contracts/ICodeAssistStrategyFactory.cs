using AIAssistant.Models;

namespace AIAssistant.Contracts;

public interface ICodeAssistStrategyFactory
{
    ICodeStrategy Create(CodeAssistStrategyType codeAssistStrategyType);
}
