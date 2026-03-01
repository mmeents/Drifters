using Drifters.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Drifters.Core.Handlers.DrifterEngine {

  public record SeedRunDefaultsCommand() : IRequest<bool>;
  public class SeedRunDefaultsCommandHandler : IRequestHandler<SeedRunDefaultsCommand, bool> {
    private readonly DriftersDbContext db;

    public SeedRunDefaultsCommandHandler(DriftersDbContext db) {
      this.db = db;
    }

    public async Task<bool> Handle(SeedRunDefaultsCommand request, CancellationToken ct) {

      SeedConfig? config = null;
      config ??= DefaultSeed();

      var run = new Run {
        Name = config.RunName,
        InitialScenario = config.InitialScenario,
        SetDesignerModel = config.SetDesignerModel,
        MaxTicks = config.MaxTicks,
        Status = RunStatus.Pending,
        CreatedAt = DateTime.UtcNow
      };

      db.Runs.Add(run);
      await db.SaveChangesAsync(ct);

      for (int i = 0; i < config.Characters.Count; i++) {
        var charConfig = config.Characters[i];
        var character = new Character {
          RunId = run.Id,
          Name = charConfig.Name,
          Model = charConfig.Model ?? config.SetDesignerModel,
          SystemPrompt = charConfig.SystemPrompt,
          Objectives = charConfig.Objectives,
          Motives = charConfig.Motives,
          Rank = i + 1
        };
        db.Characters.Add(character);
      }

      await db.SaveChangesAsync(ct);

      return await Task.FromResult(true);
    }

    private static SeedConfig DefaultSeed() => new() {
      RunName = "The First Drift",
      InitialScenario = "A fog-covered crossroads at the edge of a ruined city. Three paths lead into the unknown.",
      SetDesignerModel = "liquid/lfm2.5-1.2b",
      MaxTicks = 10,
      Characters =
       [
           new()
            {
                Name = "Seraph",
                Model = "liquid/lfm2.5-1.2b",
                SystemPrompt = "You are Seraph, a cautious archivist who moves carefully through the world. You value knowledge above all things and believe that understanding the past is the key to surviving the future.",
                Objectives = "Recover the lost index of the Archive of Whispers",
                Motives = "Fear of knowledge being erased forever"
            },
            new()
            {
                Name = "Cinder",
                Model = "liquid/lfm2.5-1.2b",
                SystemPrompt = "You are Cinder, an impulsive salvager who acts first and thinks later. You have a nose for valuable things and a gift for finding treasure in ruins.",
                Objectives = "Find enough treasure to buy passage out of the dead city",
                Motives = "Fear of being forgotten and dying unknown"
            }
       ]
    };

  }

  public class SeedConfig {
    public string RunName { get; set; } = string.Empty;
    public string InitialScenario { get; set; } = string.Empty;
    public string SetDesignerModel { get; set; } = "liquid/lfm2.5-1.2b";
    public int MaxTicks { get; set; } = 10;
    public List<CharacterSeedConfig> Characters { get; set; } = new List<CharacterSeedConfig>();
  }

  public class CharacterSeedConfig {
    public string Name { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
    public string Objectives { get; set; } = string.Empty;
    public string Motives { get; set; } = string.Empty;
  }
}
