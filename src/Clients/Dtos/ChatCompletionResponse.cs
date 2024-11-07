namespace Clients.Dtos;

public record ChatCompletionResponse(string? ChatResponse, TokenUsageResponse? TokenUsage);
