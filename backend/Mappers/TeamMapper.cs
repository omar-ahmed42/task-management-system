using backend.Dtos.Teams;
using Riok.Mapperly.Abstractions;

namespace backend.Mappers;

[Mapper]
public static partial class TeamMapper
{
    public static partial Entities.Team ToTeam(TeamCreation team);

}
