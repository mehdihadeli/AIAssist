using Clients.Models;

namespace Clients.Dtos;

public record ChatCompletionRequest(IEnumerable<ChatCompletionRequestItem> Items);

public record ChatCompletionRequestItem(RoleType Role, string Prompt);
