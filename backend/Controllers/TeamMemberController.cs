using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Dtos.Teams.Members;
using backend.Entities;
using backend.Mappers;
using backend.Security.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class TeamMemberController(TaskManagementDbContext dbContext) : ControllerBase
    {

        private readonly TaskManagementDbContext _dbContext = dbContext;

        [HttpGet("teams/{team-id:Guid}/members", Name = "GetTeamMembers")]
        [Authorize]
        public async Task<ActionResult<List<TeamMemberDetails>>> GetTeamMembers([FromRoute(Name = "team-id")] string teamId)
        {

            var team = await _dbContext.Teams.FindAsync(teamId);
            if (team == null) return NotFound(new ErrorResponse("TEAM_NOT_FOUND", "Team not found"));

            var principalId = User.GetUserId();
            if (User.IsAdmin() || IsTeamLeader(team, principalId) || await IsTeamMember(teamId, principalId))
            {

                var teamMembers = await _dbContext.TeamsMembers
                .Where(tm => teamId.Equals(tm.TeamId))
                .Include(tm => tm.Member)
                .AsNoTracking().ToListAsync();

                return Ok(value: TeamMemberMapper.ToTeamMemberDetailsList(teamMembers));
            }

            return StatusCode(403, new ErrorResponse("TEAM_FORBID", "You cannot access this resource"));
        }

        private static bool IsTeamLeader(Team team, Guid? leaderId)
        {
            return team != null && team.LeaderId != null && leaderId != null && team.LeaderId.Equals(leaderId);
        }

        private async Task<bool> IsTeamMember(string teamId, Guid? memberId)
        {
            if (teamId == null || memberId == null) return false;
            var teamMember = await _dbContext.TeamsMembers.FindAsync(teamId, memberId);
            return teamMember != null;
        }

        [HttpGet("teams/{team-id:Guid}/members/{member-id:Guid}", Name = "GetTeamMember")]
        [Authorize]
        public async Task<ActionResult<TeamMemberDetails>> GetTeamMember([FromRoute(Name = "team-id")] string teamId, [FromRoute(Name = "member-id")] Guid memberId)
        {
            var team = await _dbContext.Teams.FindAsync(teamId);
            if (team == null) return NotFound(new ErrorResponse("TEAM_NOT_FOUND", "Team not found"));

            var principalId = User.GetUserId();
            if (User.IsAdmin() || IsTeamLeader(team, principalId) || await IsTeamMember(teamId, principalId))
            {

                var teamMember = await _dbContext.TeamsMembers.FindAsync(teamId, memberId);
                if (teamMember == null) return NotFound(new ErrorResponse("MEMBER_NOT_FOUND", "Team member not found"));
                return Ok(TeamMemberMapper.ToTeamMemberDetails(teamMember));
            }

            return StatusCode(403, new ErrorResponse("TEAM_FORBID", "You cannot access this resource"));
        }
    }
}
