using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using Moq;
using StudentManagement.Agent.Services;
using StudentManagement.Agent.Services.Models;

namespace StudentManagement.UnitTests.Agent;

/// <summary>
/// Tests for <see cref="StudentManagementAgent.ProcessAsync"/> covering the
/// LLM-response relay path. OCR and MCP tool paths are tested at the
/// integration level; here we only verify chat-client interactions.
/// </summary>
public sealed class AgentOrchestratorTests
{
    private readonly Mock<IChatClient> _chatClient = new();

    private StudentManagementAgent BuildAgent()
    {
        // Constructor: (Lazy<McpClient>, ILogger, IChatClient?, AzureDocumentIntelligenceService?)
        var agent = new StudentManagementAgent(
            new Lazy<Task<McpClient>>(() => Task.FromResult<McpClient>(null!)),
            NullLogger<StudentManagementAgent>.Instance,
            chat: _chatClient.Object,
            ocr: null);

        // Pre-set the instance _cachedTools to an empty list so the agent never
        // reaches McpClient.ListToolsAsync (McpClient is null above).
        typeof(StudentManagementAgent)
            .GetField("_cachedTools", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(agent, (IList<McpClientTool>)new List<McpClientTool>());

        return agent;
    }

    private static AgentRequest MakeRequest(string message) =>
        new(message, []);

    // ── helpers ──────────────────────────────────────────────────────────

    private void SetupChatClientReply(string text)
    {
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, text));

        _chatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    // ── test methods ─────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_UnclearIntent_ReturnsClarificationQuestion()
    {
        // LLM returns a clarifying question when the intent is ambiguous.
        const string question = "Hangi öğrencinin kaydını güncellemek istiyorsunuz?";
        SetupChatClientReply(question);
        var agent = BuildAgent();
        var request = MakeRequest("güncelle lütfen");

        var response = await agent.ProcessAsync(request, CancellationToken.None);

        response.Reply.Should().Be(question);
        response.RequiresConfirmation.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_NormalMessage_RelaysLlmReply()
    {
        // The agent should relay whatever text the LLM returns and call
        // GetResponseAsync exactly once per request.
        const string expectedReply = "Ali Veli'nin notu 90 olarak güncellendi.";
        SetupChatClientReply(expectedReply);
        var agent = BuildAgent();
        var request = MakeRequest("Ali Veli notunu 90 yap");

        var response = await agent.ProcessAsync(request, CancellationToken.None);

        response.Reply.Should().Be(expectedReply);
        _chatClient.Verify(
            c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_LlmThrows_PropagatesException()
    {
        // If the chat client fails the exception must bubble up; the agent
        // must not swallow errors or return a partial response.
        _chatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("LLM bağlantısı başarısız"));
        var agent = BuildAgent();
        var request = MakeRequest("bir şey yap");

        Func<Task> act = () => agent.ProcessAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*LLM bağlantısı başarısız*");
    }
}
