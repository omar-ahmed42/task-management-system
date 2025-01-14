using backend.Dtos.Teams.Members;
using backend.Entities;
using Riok.Mapperly.Abstractions;

namespace backend.Mappers;

[Mapper]
public static partial class TeamMemberMapper
{

    [MapNestedProperties(nameof(TeamMember.Member))]
    [MapperIgnoreSource(nameof(TeamMember.Team))]
    [MapperIgnoreSource(nameof(TeamMember.MemberId))]
    public static partial TeamMemberDetails ToTeamMemberDetails(TeamMember teamMember);

    public static partial List<TeamMemberDetails> ToTeamMemberDetailsList(List<TeamMember> teamMembers);


}
