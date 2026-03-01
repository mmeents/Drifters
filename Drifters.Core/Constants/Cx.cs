using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drifters.Core.Constants {
  public static class Cx {
    public static string AppName => "Drifters";
    public static string AppVersion => "0.1.0"; 

    public const string WorldDesignerAppName = "WorldDesignerMCP";
    public const string CharacterDesignerAppName = "CharacterDesignerMCP";

    public const string ApiLocalhostUrl = "http://localhost:4300";
    public const string CharacterLMStudioUrl = "http://10.0.0.118:8669";
    public const string WorldStateLMStudioUrl = "http://10.0.0.247:8669";

    // ── Set Designer ─────────────────────────────────────────────────────────

    public const string SetDesignerSystemPrompt =
      "You are the Set Designer of an autonomous narrative engine called Drifters. " +
      "You describe a living world in vivid present tense, second person plural ('Before you...', 'The air hangs heavy...'). " +
      "You are called twice each story tick: " +
      "SCENE: paint the environment and tension, then list exactly the five tool decisions. " +
      "CONTINUATION: after characters have acted, weave their choices into the next story beat, then call update-world-state. " +
      "Never break character. Never explain the engine, the tools, or your role. Never number the decisions.";

    // Scene prompt — the format block is shown as a literal example so small models copy it exactly.
    public const string SetDesignerSceneQuestion =
      "Describe the current scene in vivid detail (environment, atmosphere, sensory details, immediate tension). " +
      "Keep the description under 250 words. " +
      "Then end with the DECISIONS block in EXACTLY this format — no numbering, no extra text after it:\n\n" +
      "DECISIONS:\n" +
      "explore: <one evocative sentence — what exploring would reveal here>\n" +
      "examine: <one evocative sentence — what examining closely would uncover>\n" +
      "take_action: <one evocative sentence — what acting decisively would accomplish>\n" +
      "speak: <one evocative sentence — what speaking aloud might change>\n" +
      "wait_and_observe: <one evocative sentence — what patient watching would show>\n\n" +
      "The five tool names (explore, examine, take_action, speak, wait_and_observe) must appear exactly as written.";

    // Continuation prompt — instructs the model to narrate consequences then call update-world-state.
    public const string SetDesignerContinuationQuestion =
      "The characters have acted. Their decisions and the world's responses are listed above.\n\n" +
      "Write a continuation passage (100-200 words) that:\n" +
      "1. Shows what happened as a result of each character's action, weaving them together\n" +
      "2. Describes how the world has visibly shifted — light, sound, atmosphere, position\n" +
      "3. Ends on a new moment of tension or discovery that makes the next scene inevitable\n\n" +
      "Write in vivid present tense, second person plural. " +
      "Do not list the next decisions — the next SCENE call handles that. " +
      "Do not break character.\n\n" +
      "After the passage, use mcp tool driftersworldmcp call update-world-state with following JSON shape:\n" +
      "{\n" +
      "  \"tick\": <current tick number>,\n" +
      "  \"location\": \"<where the characters now are>\",\n" +
      "  \"mood\": \"<dominant atmosphere or tone>\",\n" +
      "  \"keyEvents\": [\"<one sentence per major thing that happened>\"],\n" +
      "  \"characterStates\": { \"<CharacterName>\": \"<what changed for them>\" }\n" +
      "}\n" +
      "This JSON is the world's memory — make it accurate and specific.";

    // ── Character Agent ───────────────────────────────────────────────────────

    // Appended to every character's dynamic system prompt (name + objectives + motives).
    public const string CharacterSystemPromptSuffix =
      "You exist inside a living story. When it is your turn you will be shown the current scene.\n" +
      "You MUST respond by calling exactly one of the five available tools. " +
      "Do not write narration, do not explain your choice, do not reply with text. " +
      "Call the tool. The tool call IS your action.";

    // Injected into the character's user prompt so the tool names appear in context.
    public const string CharacterToolReminder =
      "\nthe mcp actions should be available via driftercharactermcp (please call one mcp tool):\n" +
      "  explore — move or look in a direction\n" +
      "  examine — inspect something closely\n" +
      "  take_action — do something decisive\n" +
      "  speak — say something aloud\n" +
      "  wait_and_observe — hold position and watch\n";

    // ── Tool name constants ───────────────────────────────────────────────────

    public const string CharacterServerLabel   = "character_mcp";
    public const string ExploreTool            = "explore";
    public const string ExamineTool            = "examine";
    public const string TakeActionTool         = "take_action";
    public const string SpeakTool              = "speak";
    public const string WaitAndObserveTool     = "wait_and_observe";

    // ── Paths ─────────────────────────────────────────────────────────────────

    public static string LogsAppPath {
      get {
        string logsPath = Path.Combine(CommonAppPath, "logs").ResolvePath();
        if (!Directory.Exists(logsPath)) {
          Directory.CreateDirectory(logsPath);
        }
        return logsPath;
      }
    }

    public static string CommonAppPath {
      get {
        string commonPath = Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
          Cx.AppName).ResolvePath();
        if (!Directory.Exists(commonPath)) {
          Directory.CreateDirectory(commonPath);
        }
        return commonPath;
      }
    }

    public static string ResolvePath(this string path) {
      if (!Path.IsPathRooted(path)) {
        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
      }
      return Path.GetFullPath(path);
    }
  }
}
