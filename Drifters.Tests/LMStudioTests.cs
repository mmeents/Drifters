using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Drifters.Core.Clients;
using Drifters.Core.Models; 

namespace DaemonsMCP.xUnit.Tests.AgentCli {
  /// <summary>
  /// Integration tests against a live LM Studio instance.
  /// Set LM_HOST env var or update BaseUrl if needed.
  /// These tests actually hit the server — keep them in a separate test category
  /// from pure unit tests if you want CI to skip them.
  /// </summary>
  public class LmStudioIntegrationTests(ITestOutputHelper output) : IDisposable {
    // ── Config ────────────────────────────────────────────────
    private const string BaseUrl = "http://10.0.0.118:8669";

    /// Key of the small loaded model — update to whatever is running.
    private const string SmallModel = "liquid/lfm2.5-1.2b";

    private readonly LmStudioClient _client = new();

    public void Dispose() => _client.Dispose();

    // ─────────────────────────────────────────────────────────
    // MODELS ENDPOINT
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetModels_ReturnsNonEmptyList() {
      var result = await _client.GetModelsAsync();

      Assert.NotNull(result);
      Assert.NotEmpty(result.Models);
      output.WriteLine($"Found {result.Models.Count} model(s)");
      foreach (var m in result.Models)
        output.WriteLine($"  [{m.Type}] {m.Key} — {m.SizeGb} GB  loaded={m.IsLoaded}");
    }

    [Fact]
    public async Task GetModels_LlmFilter_ExcludesEmbeddings() {
      var llms = await _client.GetLlmModelsAsync();

      Assert.All(llms, m => Assert.Equal("llm", m.Type));
      output.WriteLine($"LLM model count: {llms.Count}");
    }

    [Fact]
    public async Task GetModels_ModelFieldsPopulated() {
      var result = await _client.GetModelsAsync();
      var first = result.Models.First();

      Assert.NotEmpty(first.Key);
      Assert.NotEmpty(first.DisplayName);
      Assert.True(first.SizeBytes > 0);
      Assert.True(first.MaxContextLength > 0);
    }

    [Fact]
    public async Task GetModels_LoadedInstanceCheck() {
      var result = await _client.GetModelsAsync();

      // liquid/lfm2.5-1.2b should be loaded per the sample JSON
      var loaded = result.Models.Where(m => m.IsLoaded).ToList();
      output.WriteLine($"Loaded models: {loaded.Count}");
      foreach (var m in loaded)
        output.WriteLine($"  {m.Key}  instances={m.LoadedInstances.Count}");

      // At least one model should be loaded if server is warm
      Assert.NotEmpty(loaded);
    }

    // ─────────────────────────────────────────────────────────
    // CHAT ENDPOINT — basic
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Chat_SimpleStringInput_ReturnsMessage() {
      var response = await _client.ChatAsync(SmallModel, "Reply with exactly: HELLO");

      Assert.NotNull(response);
      Assert.NotEmpty(response.Output);
      Assert.NotEmpty(response.GetText());
      output.WriteLine($"Response: {response.GetText()}");
    }

    [Fact]
    public async Task Chat_SystemPrompt_IsRespected() {
      var response = await _client.ChatAsync(
          model: SmallModel,
          prompt: "What is 2 + 2?",
          systemPrompt: "You are a pirate. Always answer in pirate speak.",
          temperature: 0
      );

      Assert.NotEmpty(response.GetText());
      output.WriteLine($"Pirate answer: {response.GetText()}");
    }

    [Fact]
    public async Task Chat_StatsReturned() {
      var response = await _client.ChatAsync(SmallModel, "Say hi.");

      Assert.NotNull(response.Stats);
      Assert.True(response.Stats!.InputTokens > 0);
      Assert.True(response.Stats.TotalOutputTokens > 0);
      Assert.True(response.Stats.TokensPerSecond > 0);
      output.WriteLine($"Tokens/sec: {response.Stats.TokensPerSecond:F1}");
    }

    [Fact]
    public async Task Chat_ResponseIdReturnedWhenStoreTrue() {
      var request = ChatRequest.Simple(SmallModel, "this is a test to inspect the response structure. Ping");
      request.Store = true;

      var response = await _client.ChatAsync(request);

      Assert.NotNull(response.ResponseId);
      Assert.StartsWith("resp_", response.ResponseId);
      output.WriteLine($"Response ID: {response.ResponseId}");
    }

    [Fact]
    public async Task Chat_StoreFalse_NoResponseId() {
      var request = ChatRequest.Simple(SmallModel, "Question, are you able to see the daemonsmcp tools.  simple yes or no.");
      request.Store = false;

      var response = await _client.ChatAsync(request);

      Assert.Null(response.ResponseId);
      output.WriteLine("Response: "+response.GetText());
    }

    // ─────────────────────────────────────────────────────────
    // CHAT ENDPOINT — multi-item input
    // ─────────────────────────────────────────────────────────

    //[// Fact ]  -- maybe someday, current model does not support images.
    public async Task Chat_MultiItemInput_Works() {
      var items = new List<InputItem>
      {
            new TextInputItem { Content = "What color is this image?" },
            // 1x1 red pixel
            new ImageInputItem
            {
                DataUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwADhQGAWjR9awAAAABJRU5ErkJggg=="
            }
        };

      // Use a vision-capable model if available; otherwise expect graceful failure
      const string visionModel = "mistralai/ministral-3-3b";
      var request = ChatRequest.WithItems(visionModel, items);
      request.ContextLength = 2048;
      request.Temperature = 0;

      var response = await _client.ChatAsync(request);
      output.WriteLine($"Vision response: {response.GetText()}");
      Assert.NotEmpty(response.Output);
    }

    // ─────────────────────────────────────────────────────────
    // CHAT ENDPOINT — MCP integration
    // ─────────────────────────────────────────────────────────

    //[//Fact] -- will come back to this. Currently mcp tools are configured and available from the server.
    public async Task Chat_WithDaemonsMcpIntegration_ToolCallAppears() {
      // Adjust server_url to wherever your DaemonsMCP server is listening
      const string daemonsUrl = "http://localhost:5000/mcp"; // example

      var request = ChatRequest.Simple(SmallModel,
          "List the available projects using the list-projects tool.");

      request.Integrations =
      [
          new EphemeralMcpIntegration
            {
                ServerLabel = "daemons",
                ServerUrl   = daemonsUrl,
                AllowedTools = ["list-projects"]
            }
      ];
      request.ContextLength = 8000;
      request.Temperature = 0;

      var response = await _client.ChatAsync(request);

      var toolCalls = response.Output.OfType<ToolCallOutputItem>().ToList();
      output.WriteLine($"Tool calls: {toolCalls.Count}");
      foreach (var tc in toolCalls)
        output.WriteLine($"  {tc.Tool} → {tc.Output?[..Math.Min(120, tc.Output.Length)]}");

      // Model should have attempted at least one tool call
      Assert.NotEmpty(toolCalls);
      Assert.All(toolCalls, tc => Assert.Equal("list-projects", tc.Tool));
    }

    // ─────────────────────────────────────────────────────────
    // CHAT ENDPOINT — conversation continuity
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Chat_PreviousResponseId_ContinuesConversation() {
      // Turn 1
      var req1 = ChatRequest.Simple(SmallModel, "My lucky number is 42. Remember it.");
      req1.Store = true;
      var resp1 = await _client.ChatAsync(req1);
      Assert.NotNull(resp1.ResponseId);

      // Turn 2 — continues from turn 1
      var req2 = ChatRequest.Simple(SmallModel, "What was my lucky number?");
      req2.PreviousResponseId = resp1.ResponseId;

      var resp2 = await _client.ChatAsync(req2);
      output.WriteLine($"Continuation response: {resp2.GetText()}");

      Assert.Contains("42", resp2.GetText());
    }
  }

}
