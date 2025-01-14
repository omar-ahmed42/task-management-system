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

                var teamMember = await _dbContext.TeamsMembers.Include(tm => tm.Member).FirstOrDefaultAsync(tm => teamId.Equals(tm.TeamId) && memberId.Equals(tm.MemberId));
                if (teamMember == null) return NotFound(new ErrorResponse("MEMBER_NOT_FOUND", "Team member not found"));
                return Ok(TeamMemberMapper.ToTeamMemberDetails(teamMember));
            }

            return StatusCode(403, new ErrorResponse("TEAM_FORBID", "You cannot access this resource"));
        }

        [HttpPost("teams/{team-id:Guid}/members", Name = "AddTeamMember")]
        [Authorize]
        public async Task<ActionResult> AddTeamMember(TeamMemberCreation memberCreation)
        {
            var team = await _dbContext.Teams.FindAsync(memberCreation.TeamId);
            if (team == null) return NotFound(new ErrorResponse("TEAM_NOT_FOUND", "Team not found"));

            var principalId = User.GetUserId();
            if (User.IsAdmin() || IsTeamLeader(team, principalId))
            {
                var teamMember = await _dbContext.TeamsMembers.FindAsync(memberCreation.TeamId, memberCreation.Details.MemberId);
                if (teamMember != null) return Conflict(new ErrorResponse("MEMBER_ALREADY_EXISTS", "This user is already a member of the team"));

                DateTime joinedAt;
                if (User.IsAdmin() && memberCreation.Details.JoinedAt != null) joinedAt = (DateTime) memberCreation.Details.JoinedAt;
                else joinedAt = DateTime.UtcNow;

                teamMember = new TeamMember() { TeamId = memberCreation.TeamId, MemberId = memberCreation.Details.MemberId, JoinedAt = joinedAt};
                await _dbContext.AddAsync(teamMember);
                await _dbContext.SaveChangesAsync();
                return CreatedAtRoute("GetTeamMember", new RouteValueDictionary { { "team-id", memberCreation.TeamId }, { "member-id", memberCreation.Details.MemberId } }, null);
            }

            return StatusCode(403, new ErrorResponse("TEAM_FORBID", "You cannot access this resource"));
        }
    }
}
