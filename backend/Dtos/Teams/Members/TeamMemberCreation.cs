using Microsoft.AspNetCore.Mvc;

namespace backend.Dtos.Teams.Members;

public class TeamMemberCreation
{
    [FromRoute(Name = "team-id")]
    public required string TeamId { get; set; }

    [FromBody]
    public required TeamMemberCreationDetails Details { get; set; }
}
