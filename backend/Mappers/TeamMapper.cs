using backend.Dtos.Teams;
using Riok.Mapperly.Abstractions;

namespace backend.Mappers;

[Mapper]
public static partial class TeamMapper
{
    public static partial Entities.Team ToTeam(TeamCreation team);
    public static partial TeamResponse ToTeamResponse(Entities.Team team);

    public static partial void MergeTeam(TeamUpdateDetails taskDetails, Entities.Team team);

}
