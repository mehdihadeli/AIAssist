using Clients.Chat.Models;
using Clients.Models;

namespace Clients.UnitTests.Models;

public class ChatSessionTests
{
    [Fact]
    public void AddUserChatItem_ShouldAddToChatHistoryCorrectly()
    {
        // Arrange
        var chatSession = new ChatSession();
        var userPrompt = "Hello, how are you?";

        // Act
        var userChatItem = chatSession.AddUserChatItem(userPrompt);

        // Assert
        Assert.NotNull(userChatItem);
        Assert.Equal(RoleType.User, userChatItem.Role);
        Assert.Equal(userPrompt, userChatItem.Prompt);
        Assert.NotNull(chatSession.ChatHistory);
        Assert.NotEmpty(chatSession.ChatHistory.HistoryItems);
        Assert.Equal(chatSession.ChatHistory.HistoryItems[0].Prompt, userPrompt);
    }

    [Fact]
    public void AddSystemChatItem_ShouldAddToChatHistoryCorrectly()
    {
        // Arrange
        var chatSession = new ChatSession();
        var systemPrompt = "System update message";
        var context = "Some context";

        // Act
        var systemChatItem = chatSession.AddSystemChatItem(systemPrompt);

        // Assert
        Assert.NotNull(systemChatItem);
        Assert.Equal(RoleType.System, systemChatItem.Role);
        Assert.Equal(systemPrompt, systemChatItem.Prompt);
        Assert.NotNull(chatSession.ChatHistory);
        Assert.NotEmpty(chatSession.ChatHistory.HistoryItems);
        Assert.Equal(chatSession.ChatHistory.HistoryItems[0].Prompt, systemPrompt);
    }

    [Fact]
    public void AddAssistantChatItem_ShouldAddToChatHistoryCorrectly()
    {
        // Arrange
        var chatSession = new ChatSession();
        var assistantPrompt = "Here is the information you requested.";

        // Act
        var assistantChatItem = chatSession.AddAssistantChatItem(assistantPrompt);

        // Assert
        Assert.NotNull(assistantChatItem);
        Assert.Equal(RoleType.Assistant, assistantChatItem.Role);
        Assert.Equal(assistantPrompt, assistantChatItem.Prompt);
        Assert.NotNull(chatSession.ChatHistory);
        Assert.NotEmpty(chatSession.ChatHistory.HistoryItems);
        Assert.Equal(chatSession.ChatHistory.HistoryItems[0].Prompt, assistantPrompt);
    }

    [Fact]
    public void SessionId_ShouldBeUniqueForEachSession()
    {
        // Arrange
        var chatSession1 = new ChatSession();
        var chatSession2 = new ChatSession();

        // Act & Assert
        Assert.NotEqual(chatSession1.SessionId, chatSession2.SessionId);
    }

    [Fact]
    public void ChatHistory_ShouldStoreMultipleChatItems()
    {
        // Arrange
        var chatSession = new ChatSession();

        var userPrompt = "User question";
        var systemPrompt = "System update";
        var assistantPrompt = "Assistant response";

        // Act
        chatSession.AddUserChatItem(userPrompt);
        chatSession.AddSystemChatItem(systemPrompt);
        chatSession.AddAssistantChatItem(assistantPrompt);

        var history = chatSession.ChatHistory.HistoryItems;

        // Assert
        Assert.Equal(3, history.Count); // Assuming GetHistory() returns a collection of ChatItems
        Assert.Contains(history, item => item.Prompt == userPrompt && item.Role == RoleType.User);
        Assert.Contains(history, item => item.Prompt == systemPrompt && item.Role == RoleType.System);
        Assert.Contains(history, item => item.Prompt == assistantPrompt && item.Role == RoleType.Assistant);
    }
}
