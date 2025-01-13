using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Teams;

public record class TeamCreation([Required][MaxLength(50)] string Name, [Required][MaxLength(250)] string Description, Guid? LeaderId);