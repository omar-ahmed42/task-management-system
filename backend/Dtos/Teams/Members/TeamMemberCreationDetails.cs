namespace backend.Dtos.Teams.Members;

public class TeamMemberCreationDetails
{
    public Guid MemberId { get; set; }
    public DateTime? JoinedAt{ get; set; }
}
