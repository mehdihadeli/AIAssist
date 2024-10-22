using AIAssistant.Contracts;
using AIAssistant.Models;

namespace AIAssistant.Services.CodeAssistStrategies;

public class CodeAssistStrategyFactory(IDictionary<CodeAssistStrategyType, ICodeStrategy> strategies)
    : ICodeAssistStrategyFactory
{
    public ICodeStrategy Create(CodeAssistStrategyType codeAssistStrategyType)
    {
        return strategies[codeAssistStrategyType];
    }
}
