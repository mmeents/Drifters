# CLAUDE_CODE_BRIEF.md
# Drifters — Full Build Brief for Claude Code

## What You Are Building

**Drifters** is a multi-agent narrative engine. A **Set Designer** LLM describes a living
world and generates scenes that end with decisions. Multiple **Character** agents (each
with unique motives and objectives) react to the scene and make decisions via MCP tool
calls. Their decisions are resolved, the world state updates, the Set Designer writes a
continuation, and the loop repeats — indefinitely, overnight if desired.

The result is a fully logged, replayable, choose-your-own-adventure story that writes
itself, stored entirely in SQL Server.

---

## Solution Structure

Create the following projects inside a `Drifters.sln`:

```
Drifters/
├── Drifters.sln
├── CLAUDE_CODE_BRIEF.md               ← this file
├── SETUP.md
│
├── Drifters.Core/                     ← Domain models, interfaces, EF Core
│   ├── Drifters.Core.csproj
│   ├── DriftersDbContext.cs
│   ├── Models/                       <- dtos>   
│   ├── Entities/                     <- EF Core entities, if you want to separate from models
│   │   ├── Run.cs
│   │   ├── RunConfiguration.cs       <- please create the configuration next to entities foreach entity, so we can include by assembly below.>
│   │   ├── Tick.cs
│   │   ├── Character.cs
│   │   ├── Turn.cs
│   │   ├── WorldState.cs
│   │   └── ToolEventLog.cs
│   └── Migrations/                    ← EF Core migrations go here
│
├── Drifters.LmStudio/                 ← LM Studio HTTP client (already written)
│   ├── Drifters.LmStudio.csproj
│   ├── LmStudioClient.cs
│   └── LmStudioModels.cs
├── Drifters.Tests/                 ← LM Studio integration tests (already written)
│   └── LmStudioTests.cs
│
├── Drifters.Engine/                   ← The loop runner
│   ├── Drifters.Engine.csproj
│   ├── DriftEngine.cs                 ← Orchestrates ticks
│   ├── SetDesignerAgent.cs
│   ├── CharacterAgent.cs
│   └── EngineConfig.cs
│
├── Drifters.WorldMcp/                 ← MCP server: world state tools for Set Designer
│   ├── Drifters.WorldMcp.csproj
│   └── Program.cs
│
├── Drifters.CharacterMcp/             ← MCP server: decision/option tools for Characters
│   ├── Drifters.CharacterMcp.csproj
│   └── Program.cs
│
├── Drifters.Runner/                   ← Console host: starts a run, runs the loop
│   ├── Drifters.Runner.csproj
│   ├── Program.cs
│   └── appsettings.json
│
└── Drifters.App/                      ← Blazor Server app: watch the story unfold live
    ├── Drifters.App.csproj
    ├── Program.cs
    ├── Components/
    │   ├── Pages/
    │   │   ├── Home.razor             ← List of Runs
    │   │   ├── RunView.razor          ← Watch a run tick by tick
    │   │   └── TickDetail.razor       ← Deep dive: scene, each character turn, tool calls
    │   └── Layout/
    └── appsettings.json
```

---

## NuGet Packages

### Drifters.Core
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.*" />
```

### Drifters.WorldMcp + Drifters.CharacterMcp
```xml
<PackageReference Include="ModelContextProtocol" Version="0.*" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.*" />
```

### Drifters.Runner
```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.*" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.*" />
```

### Drifters.App
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.*" />
```

---

## Database Models (Drifters.Core/Models/)

### Run.cs
```csharp
public class Run
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InitialScenario { get; set; } = string.Empty;  // seed prompt for tick 0
    public string LmStudioBaseUrl { get; set; } = "http://10.0.0.118:8669";
    public string SetDesignerModel { get; set; } = string.Empty;
    public int MaxTicks { get; set; } = 20;
    public RunStatus Status { get; set; } = RunStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<Character> Characters { get; set; } = [];
    public ICollection<Tick> Ticks { get; set; } = [];
}

public enum RunStatus { Pending, Running, Completed, Faulted }
```

### Character.cs
```csharp
public class Character
{
    public int Id { get; set; }
    public int RunId { get; set; }
    public Run Run { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;        // which LM Studio model to use
    public string SystemPrompt { get; set; } = string.Empty; // personality/voice
    public string Objectives { get; set; } = string.Empty;   // what they want
    public string Motives { get; set; } = string.Empty;      // why they want it
    public int Rank { get; set; }                            // turn order within a tick

    public ICollection<Turn> Turns { get; set; } = [];
}
```

### Tick.cs
```csharp
public class Tick
{
    public int Id { get; set; }
    public int RunId { get; set; }
    public Run Run { get; set; } = null!;
    public int TickNumber { get; set; }
    public string SceneDescription { get; set; } = string.Empty;   // Set Designer output
    public string? ContinuationNarrative { get; set; }             // Set Designer wrap-up
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ICollection<Turn> Turns { get; set; } = [];
    public WorldState? WorldState { get; set; }
}
```

### Turn.cs
```csharp
public class Turn
{
    public int Id { get; set; }
    public int TickId { get; set; }
    public Tick Tick { get; set; } = null!;
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public string CharacterReasoning { get; set; } = string.Empty;  // model reasoning/text
    public string? ToolCallName { get; set; }
    public string? ToolCallArguments { get; set; }   // JSON
    public string? ToolCallResult { get; set; }      // JSON or text
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### WorldState.cs
```csharp
public class WorldState
{
    public int Id { get; set; }
    public int TickId { get; set; }
    public Tick Tick { get; set; } = null!;

    // Free-form JSON blob the Set Designer maintains
    public string StateJson { get; set; } = "{}";

    // Convenience: list of decisions made this tick (denormalized for easy querying)
    public string DecisionSummary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### ToolEventLog.cs
```csharp
// Append-only log of every MCP tool invocation across all runs
public class ToolEventLog
{
    public int Id { get; set; }
    public int? TurnId { get; set; }
    public string ServerLabel { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string ArgumentsJson { get; set; } = "{}";
    public string? ResultJson { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## DriftersDbContext.cs

```csharp
public class DriftersDbContext(DbContextOptions<DriftersDbContext> options) 
    : DbContext(options)
{
    public DbSet<Run> Runs => Set<Run>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Tick> Ticks => Set<Tick>();
    public DbSet<Turn> Turns => Set<Turn>();
    public DbSet<WorldState> WorldStates => Set<WorldState>();
    public DbSet<ToolEventLog> ToolEventLogs => Set<ToolEventLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);    
      modelBuilder.ApplyConfigurationsFromAssembly(typeof(DriftersDbContext).Assembly);    
    }    
}
```

After writing the models, run:
```bash
cd Drifters.Core
dotnet ef migrations add InitialCreate --startup-project ../Drifters.Runner
dotnet ef database update --startup-project ../Drifters.Runner
```

---

## Engine Config (Drifters.Engine/EngineConfig.cs)

```csharp
public class EngineConfig
{
    public string LmStudioBaseUrl { get; set; } = "http://10.0.0.118:8669";
    public string WorldMcpUrl { get; set; } = "http://localhost:6100/mcp";
    public string CharacterMcpUrl { get; set; } = "http://localhost:6101/mcp";
    public int DelayBetweenTurnsMs { get; set; } = 500;
    public int DelayBetweenTicksMs { get; set; } = 2000;
}
```

---

## Engine Loop (Drifters.Engine/DriftEngine.cs)

Implement the following flow. Write the full class.

```
DriftEngine.RunAsync(int runId, CancellationToken ct):

1. Load Run + Characters from DB (ordered by Character.Rank)
2. Mark Run.Status = Running, Run.StartedAt = now, save

3. LOOP until MaxTicks reached or ct cancelled:

   A. Create new Tick record (TickNumber = last + 1), save to get Id

   B. SET DESIGNER TURN:
      - Build prompt:
          * System: "You are the Set Designer of a living world called Drifters. 
                     You describe scenes in vivid present tense. Every scene ends 
                     with exactly [N] named decisions that map to available tools. 
                     Never break character."
          * User: [previous continuation narrative or InitialScenario if tick 0]
                  + current WorldState.StateJson
                  + "Available decisions (tool names): [list from CharacterMcp]"
                  + "Describe the current scene and end with the decisions."
      - Call LmStudioClient with SetDesignerModel
      - Save result to Tick.SceneDescription

   C. CHARACTER TURNS (iterate in Rank order):
      - For each Character:
          * Build prompt:
              System: Character.SystemPrompt + objectives + motives
              User: Tick.SceneDescription
                    + "Your options (choose one by calling the tool): [tool list]"
          * Call LmStudioClient with Character.Model
            integrations: [ CharacterMcp ephemeral server ]
          * Extract tool call from response output
          * Save Turn record (reasoning, tool name, args, result)
          * Log to ToolEventLog

   D. SET DESIGNER CONTINUATION:
      - Build prompt:
          * System: same as B
          * User: [scene description]
                  + [summary of each character's decision and tool result]
                  + "The consequences unfold. Write the continuation that bridges 
                     to the next moment. Update the world state."
      - Call LmStudioClient
      - Save to Tick.ContinuationNarrative
      - Call WorldMcp: update_world_state with new state JSON

   E. Save WorldState record for this tick
   F. Mark Tick.CompletedAt, save
   G. await Task.Delay(DelayBetweenTicksMs)

4. Mark Run.Status = Completed, Run.CompletedAt = now, save
```

---

## Drifters.WorldMcp (MCP Server for Set Designer)

Use the `ModelContextProtocol` SDK. Expose these tools:

```
get_world_state()
    → Returns current WorldState.StateJson for the active run

update_world_state(string stateJson)
    → Overwrites WorldState.StateJson, returns "updated"

get_tick_history(int lastN = 3)
    → Returns last N tick scene descriptions + continuation narratives as context

record_scene(string sceneText)
    → Saves scene text to current tick (used internally by engine)
```

Wire to SQL Server via DriftersDbContext. Listen on port **6100**.

---

## Drifters.CharacterMcp (MCP Server for Characters/Actors)

Expose these tools — these ARE the decisions characters can make:

```
explore(string direction)
    → "You move [direction]. The path reveals: [world state fragment]"
    → Returns narrative flavor text drawn from WorldState

examine(string target)
    → "You examine [target] closely..."
    → Returns description pulled from or generated around world state

take_action(string action, string? target = null)
    → General purpose: "You [action] [target]..."
    → Logs to Turn, returns consequence text

speak(string message, string? toCharacter = null)
    → Character says something to the world or another character
    → Returns what they hear back

wait_and_observe()
    → Character holds position, gains information
    → Returns an observation about the current scene
```

All tools should log to `ToolEventLog`. Listen on port **6101**.

---

## Drifters.Runner (Console Host)

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Drifters": "Server=localhost;Database=Drifters;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Engine": {
    "LmStudioBaseUrl": "http://10.0.0.118:8669",
    "WorldMcpUrl": "http://localhost:6100/mcp",
    "CharacterMcpUrl": "http://localhost:6101/mcp"
  }
}
```

`Program.cs` should:
1. Build host with DI (DriftersDbContext, LmStudioClient, DriftEngine)
2. Seed a default Run if none exists:
   - Name: "The First Drift"
   - InitialScenario: "A fog-covered crossroads at the edge of a ruined city. 
                       Three paths lead into the unknown."
   - Two characters:
     * **Seraph** — cautious archivist, wants to preserve knowledge, fears loss
     * **Cinder** — impulsive salvager, wants treasure, fears being forgotten
   - SetDesignerModel: "liquid/lfm2.5-1.2b"
   - Both characters model: "liquid/lfm2.5-1.2b"
   - MaxTicks: 10
3. Start DriftEngine.RunAsync for that run
4. Print tick summaries to console as they complete

---

## Drifters.App (Blazor Server — Watch the Story Unfold)

### Home.razor
- Table of all Runs (name, status, tick count, started/completed)
- "Watch" button → navigates to RunView

### RunView.razor (`/run/{runId}`)
- Page title: Run name + status badge
- Auto-refreshes every 3 seconds while Run.Status == Running
- Shows ticks in order, newest at top
- Each tick shows:
  * Tick number + timestamp
  * Scene description (styled like a book passage — italic, dark background)
  * Character turn cards (name, reasoning excerpt, tool called + result snippet)
  * Continuation narrative (styled differently — bold, color accent)
- "Pause / Resume" button (sets a flag the engine checks)

### TickDetail.razor (`/run/{runId}/tick/{tickId}`)
- Full scene description
- Each character turn in full (all reasoning, full tool args + result)
- World state JSON viewer (collapsible)
- Full continuation narrative

### Styling
- Dark theme, terminal/noir aesthetic
- Use a monospace font for world state JSON
- Scene descriptions: parchment-like off-white text on dark slate
- Character turns: each character gets a distinct accent color
- Continuation: glowing amber text

---

## Seed Data Helper

In `Drifters.Runner`, create a `RunSeeder` class that can accept a config like this
and seed the DB, so Matt can easily start new runs without touching code:

```json
{
  "RunName": "The Iron Archive",
  "InitialScenario": "...",
  "Characters": [
    {
      "Name": "Seraph",
      "SystemPrompt": "You are a cautious archivist...",
      "Objectives": "Recover the lost index",
      "Motives": "Fear of knowledge being erased"
    }
  ]
}
```

---

## Build Order

1. `Drifters.Core` — models + DriftersDbContext + migration
2. `Drifters.LmStudio` — copy in existing LmStudioClient + LmStudioModels
3. `Drifters.Runner` — minimal host (needed for migrations startup project)
4. `Drifters.Engine` — DriftEngine + agents
5. `Drifters.WorldMcp` — MCP server on 6100
6. `Drifters.CharacterMcp` — MCP server on 6101  
7. `Drifters.App` — Blazor viewer
8. Wire Runner → start both MCP servers as background services, then run engine

---

## Notes for Claude Code

- The LmStudio models + client already exist — copy them from the provided files,
  do not rewrite them.
- Use `System.Text.Json` throughout, no Newtonsoft.
- Use EF Core 8 with SQL Server provider.
- All async methods should accept and forward `CancellationToken`.
- The MCP servers should be able to run independently so they can later be hosted
  on separate machines (SleeveDeck architecture — don't hardcode localhost assumptions).
- The engine should be resilient: if a character LLM call fails, log the error to
  ToolEventLog and continue to the next character rather than crashing the run.
- Every DB write should be try/catch with meaningful error logging.
- Keep the Blazor app read-only — it never writes to the DB, only reads.
