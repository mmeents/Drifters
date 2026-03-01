using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drifters.Core.Models;
using MCPSharp;

namespace Drifters.Core.Tools {
  public class CharacterTools {
    private static ICharacterToolsHandler GetTools() => DIServiceBridge.GetService<ICharacterToolsHandler>();

    [McpTool("explore", "Explore in a given direction.")]
    public static async Task<string> ExploreAsync(
      [Description("for example 'north', 'south', 'self', 'surroundings'.")] string direction
    ) => await GetTools().Explore(direction);

    [McpTool("examine", "Examine a target in the environment.")]  
    public static async Task<string> ExamineAsync(
      [Description("The target to examine.")] string target
    ) => await GetTools().Examine(target);

    [McpTool("take_action", "Take an action on a target.")]
    public static async Task<string> TakeActionAsync(
      [Description("The action to take.")] string action,
      [Description("The target of the action.")] string target
    ) => await GetTools().TakeAction(action, target);

    [McpTool("speak", "Speak optionally to a character")]
    public static async Task<string> SpeakAsync(
      [Description("The message to speak.")] string message,
      [Description("The character to speak to (optional).")] string? toCharacter = null
    ) => await GetTools().Speak(message, toCharacter);

    [McpTool("wait_and_observe", "Wait and observe. No parameters required.")]
    public static async Task<string> WaitAndObserveAsync() => await GetTools().WaitAndObserve();


  }
}
