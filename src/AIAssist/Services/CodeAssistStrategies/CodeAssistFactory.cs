using AIAssist.Contracts.CodeAssist;
using Clients.Models;

namespace AIAssist.Services.CodeAssistStrategies;

public class CodeAssistFactory(IDictionary<CodeAssistType, ICodeAssist> strategies) : ICodeAssistFactory
{
    public ICodeAssist Create(CodeAssistType codeAssistType)
    {
        return strategies[codeAssistType];
    }
}
