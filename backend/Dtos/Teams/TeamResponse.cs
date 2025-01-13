namespace backend.Dtos.Teams;

public record TeamResponse(string Name, string Description, Guid? LeaderId);