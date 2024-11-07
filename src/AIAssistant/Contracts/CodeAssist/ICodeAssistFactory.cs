using AIAssistant.Models;
using Clients.Models;

namespace AIAssistant.Contracts.CodeAssist;

public interface ICodeAssistFactory
{
    ICodeAssist Create(CodeAssistType codeAssistType);
}
