using Drifters.Core.Models;
using MCPSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drifters.Core.Tools {
  public class WorldDesignerTools {
    private static IWorldDesignerToolsHandler GetTools() => DIServiceBridge.GetService<IWorldDesignerToolsHandler>();

    [McpTool("get-world-state", "Get the current world state as a JSON blob.")]
    public static async Task<string> GetWorldStateAsync() => await GetTools().GetWorldState();

    [McpTool("update-world-state", "Update the world state with the given JSON blob.")]
    public static async Task<string> UpdateWorldStateAsync(string newStateJson, string decisionSummary) => await GetTools().UpdateWorldState(newStateJson, decisionSummary);

    [McpTool("record-scene", "Record a new scene with the given description.")]
    public static async Task<string> RecordSceneAsync(string sceneDescription) => await GetTools().RecordScene(sceneDescription);

    [McpTool("get-tick-history", "Get a history of recent ticks with their world states.")]
    public static async Task<string> GetTickHistoryAsync(int maxTicks) => await GetTools().GetTickHistory(maxTicks);

  }
}
