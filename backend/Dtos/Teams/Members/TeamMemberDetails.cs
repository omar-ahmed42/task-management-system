namespace backend.Dtos.Teams.Members;

public record class TeamMemberDetails(Guid Id, string FirstName, string LastName, string Email, DateTime JoinedAt, string TeamId);
