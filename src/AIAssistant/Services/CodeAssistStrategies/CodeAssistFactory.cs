using AIAssistant.Contracts.CodeAssist;
using Clients.Models;

namespace AIAssistant.Services.CodeAssistStrategies;

public class CodeAssistFactory(IDictionary<CodeAssistType, ICodeAssist> strategies) : ICodeAssistFactory
{
    public ICodeAssist Create(CodeAssistType codeAssistType)
    {
        return strategies[codeAssistType];
    }
}
