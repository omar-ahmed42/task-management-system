using backend.Data;
using backend.Dtos;
using backend.Dtos.Tasks.Assignments;
using backend.Entities;
using backend.Enums;
using backend.Security.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class TaskAssignmentController(TaskManagementDbContext dbContext) : ControllerBase
    {
        private readonly TaskManagementDbContext _dbContext = dbContext;

        [HttpPost("tasks/{task-id:Guid}/assignees", Name = "AssignTask")]
        [Authorize]
        public async Task<ActionResult> AssignTask(Dtos.Tasks.Assignments.TaskAssignmentCreation assignments)
        {
            Entities.Task? task = await _dbContext.Tasks.FindAsync(assignments.TaskId);
            if (task == null) return NotFound(new ErrorResponse("TASK_NOT_FOUND", "Task not found"));
            if (User.IsAdmin())
            {
                await SaveAssignmentsAsync(assignments);
            }
            else
            {
                Guid principalId = (Guid)User.GetUserId();

                HashSet<string> teamsLed = await _dbContext.Teams.FromSql($"SELECT t.id FROM teams t INNER JOIN tasks_assignments ta ON ta.assignment_type = {Enums.AssigneeType.Team} AND ta.assignee_id = t.id WHERE ta.task_id = {assignments.TaskId} AND t.leader_id = {principalId}").Select(t => t.Id).AsNoTracking().ToHashSetAsync();
                if (teamsLed == null || teamsLed.Count == 0) return StatusCode(403, new ErrorResponse("ASSIGNMENT_FORBID", "You cannot assign anyone to this task"));

                var unauthorizedTeams = assignments.Assignees.Where(assignee => assignee.AssigneeType == Enums.AssigneeType.Team && !teamsLed.Contains(assignee.AssigneeId.ToString())).Select(t => t.AssigneeId.ToString()).ToHashSet();
                if (unauthorizedTeams.Count > 0) return StatusCode(403, new ErrorResponse("ASSIGNMENT_FORBID", "You cannot assign the following teams " + unauthorizedTeams));

                HashSet<Guid> unauthorizedUserAssignees = await ExtractNonTeamMemberAssignees(assignments, teamsLed);
                if (unauthorizedUserAssignees.Count > 0) return StatusCode(403, new ErrorResponse("ASSIGNMENT_FORBID", "You cannot assign the following users " + unauthorizedUserAssignees + " as they are not part of any of your teams"));

                await SaveAssignmentsAsync(assignments);
            }

            return Created();
        }

        private async Task<HashSet<Guid>> ExtractNonTeamMemberAssignees(TaskAssignmentCreation assignments, HashSet<string> teamsLed)
        {
            var userAssignees = assignments.Assignees.Where(t => t.AssigneeType == Enums.AssigneeType.User).Select(t => t.AssigneeId).ToHashSet();
            var matchingUserAssignees = await _dbContext.TeamsMembers.Where(tm => teamsLed.Contains(tm.TeamId) && userAssignees.Contains(tm.MemberId)).Select(t => t.MemberId).ToHashSetAsync();
            var unauthorizedUserAssignees = userAssignees.Except(matchingUserAssignees).ToHashSet();
            return unauthorizedUserAssignees;
        }

        private async System.Threading.Tasks.Task SaveAssignmentsAsync(TaskAssignmentCreation assignments)
        {
            DateTime assignedAt = DateTime.UtcNow;
            List<Entities.TaskAssignment> taskAssignments = new(assignments.Assignees.Count);
            IEnumerable<TaskAssigneesCreation> assignees = await GetAssigneesToSave(assignments);
            foreach (TaskAssigneesCreation assignee in assignees)
            {
                taskAssignments.Add(new Entities.TaskAssignment() { TaskId = assignments.TaskId, AssigneeId = assignee.AssigneeId, AssigneeType = assignee.AssigneeType, AssignedAt = assignedAt });
            }
            await _dbContext.TasksAssignments.AddRangeAsync(taskAssignments);
            await _dbContext.SaveChangesAsync();
        }

        private async System.Threading.Tasks.Task<IEnumerable<TaskAssigneesCreation>> GetAssigneesToSave(TaskAssignmentCreation assignments)
        {
            var userAssignees = assignments.Assignees.Where(t => t.AssigneeType == Enums.AssigneeType.User).Select(t => t.AssigneeId).ToHashSet();
            var teamAssignees = assignments.Assignees.Where(t => t.AssigneeType == Enums.AssigneeType.Team).Select(t => t.AssigneeId).ToHashSet();
            var existingAssignments = await _dbContext.TasksAssignments.Where(ta => ta.TaskId == assignments.TaskId && teamAssignees.Contains(ta.AssigneeId) && ta.AssigneeType == Enums.AssigneeType.Team).Union(_dbContext.TasksAssignments.Where(ta => ta.TaskId == assignments.TaskId && userAssignees.Contains(ta.AssigneeId) && ta.AssigneeType == Enums.AssigneeType.User)).ToListAsync();

            if (existingAssignments.Count == assignments.Assignees.Count)
                return [];

            return assignments.Assignees.ExceptBy(existingAssignments.Select(a => new { a.AssigneeId, a.AssigneeType }), a => new { a.AssigneeId, a.AssigneeType });
        }

        [HttpDelete("tasks/{task-id:Guid}/assignees/teams/{assignee-id:Guid}", Name = "UnassignTeamTask")]
        [Authorize]
        public async Task<ActionResult> UnassignTeamTask([FromRoute(Name = "task-id")] string taskId, [FromRoute(Name = "assignee-id")] Guid assigneeId)
        {
            if (User.IsAdmin())
            {
                _dbContext.TasksAssignments.Remove(new Entities.TaskAssignment() { TaskId = taskId, AssigneeId = assigneeId, AssigneeType = AssigneeType.Team });
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                Guid principalId = (Guid)User.GetUserId();
                var team = await _dbContext.Teams.FindAsync(assigneeId);
                if (team == null) return NotFound(new ErrorResponse("TEAM_NOT_FOUND", "Team not found"));

                if (!principalId.Equals(team.LeaderId)) return StatusCode(403, new ErrorResponse("ASSIGNMENT_FORBID", "You cannot unassign this team"));
                _dbContext.TasksAssignments.Remove(new Entities.TaskAssignment() { TaskId = taskId, AssigneeId = assigneeId, AssigneeType = AssigneeType.Team });
                await _dbContext.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete("tasks/{task-id:Guid}/assignees/users/{assignee-id:Guid}", Name = "UnassignUserTask")]
        [Authorize]
        public async Task<ActionResult> UnassignUserTask([FromRoute(Name = "task-id")] string taskId, [FromRoute(Name = "assignee-id")] Guid assigneeId)
        {
            Guid principalId = (Guid)User.GetUserId();
            if (User.IsAdmin() || principalId.Equals(assigneeId))
            {
                _dbContext.TasksAssignments.Remove(new Entities.TaskAssignment() { TaskId = taskId, AssigneeId = assigneeId, AssigneeType = AssigneeType.User });
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                Team? team = await _dbContext.Teams.FromSql($"SELECT m FROM teams_members tm INNER JOIN teams m ON tm.team_id = m.id WHERE tm.member_id = {assigneeId} AND tm.leader_id = {principalId} LIMIT 1").FirstOrDefaultAsync();
                if (team == null) return StatusCode(403, new ErrorResponse("ASSIGNMENT_FORBID", "You cannot unassign this user"));

                _dbContext.TasksAssignments.Remove(new Entities.TaskAssignment() { TaskId = taskId, AssigneeId = assigneeId, AssigneeType = AssigneeType.User });
                await _dbContext.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpGet("tasks/{task-id:Guid}/assignees/users/{assignee-id:Guid}", Name = "GetAssignedUser")]
        [Authorize]
        public async Task<ActionResult<TaskUserAssigneeDetails>> GetAssignedUser([FromRoute(Name = "task-id")] string taskId, [FromRoute(Name = "assignee-id")] Guid assigneeId)
        {
            Guid principalId = (Guid)User.GetUserId();
            if (User.IsAdmin() || principalId.Equals(assigneeId))
            {
                TaskUserAssigneeDetails? assigneeInfo = await GetUserAssigneeDetailsAsync(taskId, assigneeId);
                if (assigneeInfo == null) return NotFound(new ErrorResponse("ASSIGNEE_NOT_FOUND", "Assigned User not found"));
                return Ok(assigneeInfo);
            }
            else
            {
                Team? team = await _dbContext.Teams.FromSql($"SELECT m FROM teams_members tm INNER JOIN teams m ON tm.team_id = m.id WHERE tm.member_id = {assigneeId} AND tm.leader_id = {principalId} LIMIT 1").FirstOrDefaultAsync();
                if (team == null) return StatusCode(403, new ErrorResponse("ASSIGNMENT_FORBID", "You cannot access this resource"));
                TaskUserAssigneeDetails? assigneeInfo = await GetUserAssigneeDetailsAsync(taskId, assigneeId);

                if (assigneeInfo == null) return NotFound(new ErrorResponse("ASSIGNEE_NOT_FOUND", "Assigned User not found"));
                return Ok(assigneeInfo);
            }
        }

        private async Task<TaskUserAssigneeDetails?> GetUserAssigneeDetailsAsync(string taskId, Guid assigneeId)
        {
            return await _dbContext.TasksAssignments.Join(_dbContext.Users, (ta) => ta.AssigneeId, (u) => u.Id, (ta, u) => new
            {
                TaskId = ta.TaskId,
                AssignedAt = ta.AssignedAt,
                AssigneeType = ta.AssigneeType,
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
            }).Where(ta => ta.TaskId == taskId && ta.AssigneeType == AssigneeType.User && ta.UserId == assigneeId)
                            .Select(ta => new TaskUserAssigneeDetails(ta.TaskId, ta.UserId, ta.FirstName, ta.LastName, ta.Email!, ta.AssignedAt)).FirstOrDefaultAsync();
        }

        [HttpGet("tasks/{task-id:Guid}/assignees/teams/{assignee-id:Guid}", Name = "GetAssignedTeam")]
        [Authorize]
        public async Task<ActionResult<TaskUserAssigneeDetails>> GetAssignedTeam([FromRoute(Name = "task-id")] string taskId, [FromRoute(Name = "assignee-id")] Guid assigneeId)
        {
            Guid principalId = (Guid)User.GetUserId();
            if (User.IsAdmin())
            {
                var assignedTeamInfo = await _dbContext.TasksAssignments.Join(_dbContext.Teams, (ta) => ta.AssigneeId.ToString(), (t) => t.Id, (ta, t) => new
                {
                    TaskId = ta.TaskId,
                    AssignedAt = ta.AssignedAt,
                    AssigneeType = ta.AssigneeType,
                    Id = assigneeId,
                    Name = t.Name,
                    Description = t.Description,
                    LeaderId = t.LeaderId,
                }).Where(t => t.TaskId == taskId && t.AssigneeType == AssigneeType.Team && t.Id == assigneeId)
                .Select(t => new TaskTeamAssigneeDetails(t.TaskId, t.Id, t.Name, t.Description, t.LeaderId))
                .FirstOrDefaultAsync();

                if (assignedTeamInfo == null) return NotFound(new ErrorResponse("ASSIGNEE_NOT_FOUND", "Assigned team not found"));
                return Ok(assignedTeamInfo);
            }
            else
            {
                var assignedTeamInfo = await _dbContext.TasksAssignments.Join(_dbContext.Teams, (ta) => ta.AssigneeId.ToString(), (t) => t.Id, (ta, t) => new
                {
                    TaskId = ta.TaskId,
                    AssignedAt = ta.AssignedAt,
                    AssigneeType = ta.AssigneeType,
                    Id = assigneeId,
                    Name = t.Name,
                    Description = t.Description,
                    LeaderId = t.LeaderId,
                }).Join(_dbContext.TeamsMembers, (ta) => ta.Id.ToString(), (tm) => tm.TeamId, (ta, tm) => new { ta, tm.MemberId })
                .Where(t => t.ta.TaskId == taskId && t.ta.AssigneeType == AssigneeType.Team && t.ta.Id == assigneeId && t.MemberId == principalId)
                .Select(t => new TaskTeamAssigneeDetails(t.ta.TaskId, t.ta.Id, t.ta.Name, t.ta.Description, t.ta.LeaderId))
                .FirstOrDefaultAsync();

                if (assignedTeamInfo == null) return NotFound(new ErrorResponse("ASSIGNEE_NOT_FOUND", "Assigned team not found"));
                return Ok(assignedTeamInfo);
            }
        }
    }
}
