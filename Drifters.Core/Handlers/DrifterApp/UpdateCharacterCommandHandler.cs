using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Drifters.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Drifters.Core.Handlers.DrifterApp {


  public record UpdateCharacterCommand(
    int CharacterId,
    string Name,    
    string SystemPrompt,
    string Objectives,
    string Motives,
    int Rank
  ) : IRequest<CharacterDto?>;
  public class UpdateCharacterCommandHandler(DriftersDbContext db) : IRequestHandler<UpdateCharacterCommand, CharacterDto?> {
    private readonly DriftersDbContext _db = db;
    public async Task<CharacterDto?> Handle(UpdateCharacterCommand request, CancellationToken cancellationToken) {
      if (request.CharacterId <= 0) {
        throw new ArgumentException("CharacterId must be greater than 0.");
      }
      var character = await _db.Characters.FirstOrDefaultAsync(c => c.Id == request.CharacterId);
      if (character == null) {
        throw new KeyNotFoundException($"Character with Id {request.CharacterId} not found.");
      }
      character.Name = request.Name;      
      character.SystemPrompt = request.SystemPrompt;
      character.Objectives = request.Objectives;
      character.Motives = request.Motives;
      character.Rank = request.Rank;

      _db.Characters.Update(character);
      await _db.SaveChangesAsync(cancellationToken);

      return new CharacterDto {
        Id = character.Id,
        Name = character.Name,        
        SystemPrompt = character.SystemPrompt,
        Objectives = character.Objectives,
        Motives = character.Motives,
        Rank = character.Rank
      };
    }
  }
}
