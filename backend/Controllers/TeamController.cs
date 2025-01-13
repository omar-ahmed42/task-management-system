using backend.Data;
using backend.Dtos;
using backend.Dtos.Teams;
using backend.Mappers;
using backend.Security.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/v1/teams")]
    [ApiController]
    public class TeamController(TaskManagementDbContext dbContext, UserManager<Entities.User> userManager) : ControllerBase
    {

        private readonly TaskManagementDbContext _dbContext = dbContext;
        private readonly UserManager<Entities.User> _userManager = userManager;

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateTeam([FromBody] TeamCreation team)
        {
            if (team == null)
                return BadRequest(new ErrorResponse("INVALID_TEAM", "Team details cannot be empty"));

            if (!User.IsAdmin()) return StatusCode(403, new ErrorResponse("TEAM_FORBIDDEN", "You cannot access this resource"));

            Entities.Team teamEntity = TeamMapper.ToTeam(team);
            teamEntity.CreatedById = (Guid)User.GetUserId();

            if (team.LeaderId != null)
            {
                var leader = await _dbContext.Users.FindAsync(team.LeaderId);
                if (leader == null) return NotFound(new ErrorResponse("LEADER_NOT_FOUND", "Team leader not found"));
                bool isAdmin = await _userManager.IsInRoleAsync(leader, "admin");
                if (isAdmin) return BadRequest(new ErrorResponse("INVALID_LEADER", "Team lead cannot be an admin"));
                teamEntity.Leader = leader;
            }

            await _dbContext.Teams.AddAsync(teamEntity);
            await _dbContext.SaveChangesAsync();

            return Created();
        }

        [HttpGet("{id}", Name = "GetTeam")]
        [Authorize]
        public async Task<ActionResult<TeamResponse>> GetTeam([FromRoute] string id)
        {
            Entities.Team? team = await _dbContext.Teams.FindAsync(id);
            if (team == null) return NotFound(new ErrorResponse("TEAM_NOT_FOUND", "Team not found"));

            if (!User.IsAdmin() && (team.LeaderId == null || (team.LeaderId != null && !team.LeaderId.Equals(User.GetUserId()))))
                return StatusCode(403, new ErrorResponse("TEAM_FORBIDDEN", "You cannot access this resource"));

            return Ok(TeamMapper.ToTeamResponse(team));
        }

        [HttpPut("{id:Guid}", Name = "UpdateTeam")]
        [Authorize]
        public async Task<ActionResult<TeamResponse>> UpdateTeam(TeamUpdate teamUpdate)
        {
            if (teamUpdate == null) return BadRequest(new ErrorResponse("INVALID_TEAM", "Team details cannot be empty"));

            Entities.Team? team = await _dbContext.Teams.FindAsync(teamUpdate.Id);
            if (team == null) return NotFound(new ErrorResponse("TEAM_NOT_FOUND", "Team not found"));

            if (!User.IsAdmin() && (team.LeaderId == null || (team.LeaderId != null && !team.LeaderId.Equals(User.GetUserId()))))
                return StatusCode(403, new ErrorResponse("TEAM_FORBIDDEN", "You cannot access this resource"));

            TeamMapper.MergeTeam(teamUpdate.Team, team);
            _dbContext.Update(team);
            await _dbContext.SaveChangesAsync();
            return Ok(TeamMapper.ToTeamResponse(team));
        }
    }
}
