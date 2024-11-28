using Clients.Models;

namespace AIAssist.Contracts.CodeAssist;

public interface ICodeAssistFactory
{
    ICodeAssist Create(CodeAssistType codeAssistType);
}
