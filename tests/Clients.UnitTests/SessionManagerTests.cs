using Clients.Models;

namespace Clients.UnitTests;

public class SessionManagerTests
{
    private readonly SessionManager _sessionManager = new();
    private readonly User _testUser = new("user1", "John Doe");
    private readonly LLM _testAssistant = new("llm1", "GPT Assistant");

    // Setup objects used across tests

    [Fact]
    public void CreateSession_ShouldCreateSessionWithSingleChat()
    {
        // Act: Create a new session
        Session session = _sessionManager.CreateSession("My Question Session", _testUser, _testAssistant);

        Assert.NotNull(session);
        Assert.NotNull(session.Chat);
        Assert.StartsWith("Session-", session.SessionName, StringComparison.InvariantCulture);

        Assert.Equal(_testUser, session.Chat.User);
        Assert.Equal(_testAssistant, session.Chat.Assistant);
    }

    [Fact]
    public void AddMessage_ShouldAddChatItemToChat()
    {
        // Arrange: Create a new session
        Session session = _sessionManager.CreateSession("My Question Session", _testUser, _testAssistant);

        // Act: Add messages to the chat
        ChatItem userMessage = new ChatItem("Hello, Assistant!", RoleType.User);
        session.Chat.AddChatItem(userMessage);

        ChatItem assistantMessage = new ChatItem("Hello, how can I assist you today?", RoleType.Assistant);

        session.Chat.AddChatItem(assistantMessage);

        // Assert: Check if the messages are added to the chat
        Assert.Equal(2, session.Chat.ChatItems.Count);
        Assert.Equal("Hello, Assistant!", session.Chat.ChatItems[0].Message);
        Assert.Equal(RoleType.User, session.Chat.ChatItems[0].Role);
        Assert.Equal("Hello, how can I assist you today?", session.Chat.ChatItems[1].Message);
        Assert.Equal(RoleType.Assistant, session.Chat.ChatItems[1].Role);
    }

    [Fact]
    public void EndSession_ShouldEndChatAndSaveToHistory()
    {
        // Arrange: Create a session and add messages
        Session session = _sessionManager.CreateSession("My Question Session", _testUser, _testAssistant);

        session.Chat.AddChatItem(new ChatItem("User's message", RoleType.User));
        session.Chat.AddChatItem(new ChatItem("Assistant's response", RoleType.Assistant));

        // Act: End the session
        _sessionManager.EndSession(session.SessionId);

        // Assert: Check that the session is removed from active sessions and history is updated
        Assert.Null(_sessionManager.GetSessionById(session.SessionId)); // Session should be removed
        var history = _sessionManager.GetSessionHistory(session.SessionId);
        Assert.NotNull(history);
        Assert.Single(history);
        Assert.Equal(2, history[0].ChatItems.Count); // Verify the messages are saved to history
    }

    [Fact]
    public void GetSessionHistory_ShouldReturnCorrectChatHistory()
    {
        // Arrange: Create a session, add messages, and end the session
        Session session = _sessionManager.CreateSession("My Question Session", _testUser, _testAssistant);

        session.Chat.AddChatItem(new ChatItem("First message", RoleType.User));
        session.Chat.AddChatItem(new ChatItem("Second message", RoleType.Assistant));
        _sessionManager.EndSession(session.SessionId);

        // Act: Retrieve the history for the ended session
        var history = _sessionManager.GetSessionHistory(session.SessionId);

        // Assert: Verify the chat history is correct
        Assert.NotNull(history);
        Assert.Single(history);
        Assert.Equal(2, history[0].ChatItems.Count);
        Assert.Equal("First message", history[0].ChatItems[0].Message);
        Assert.Equal(RoleType.User, history[0].ChatItems[0].Role);
        Assert.Equal("Second message", history[0].ChatItems[1].Message);
        Assert.Equal(RoleType.Assistant, history[0].ChatItems[1].Role);
    }

    [Fact]
    public void EndSession_ShouldThrowExceptionIfSessionNotFound()
    {
        // Act & Assert: Attempt to end a session that doesn't exist
        var exception = Assert.Throws<Exception>(() => _sessionManager.EndSession("non-existent-session-id"));

        Assert.Equal("Session not found", exception.Message);
    }
}
