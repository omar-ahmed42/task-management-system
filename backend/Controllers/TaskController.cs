using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Dtos.Tasks;
using backend.Mappers;
using backend.Security.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/v1/tasks")]
    [ApiController]
    public class TaskController(TaskManagementDbContext dbContext) : ControllerBase
    {

        private readonly TaskManagementDbContext _dbContext = dbContext;

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateTask([FromBody] TaskCreation task) {
            if (task == null) {
                return BadRequest("Task cannot be empty");
            }

            Entities.Task taskEntity = TaskMapper.ToTask(task); 
            taskEntity.CreatedById = (Guid)User.GetUserId();
            await _dbContext.Tasks.AddAsync(taskEntity);
            await _dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetTask", new {id= taskEntity.Id}, null);
        }

        [HttpGet("{id}", Name = "GetTask")]
        [Authorize]
        public async Task<ActionResult<TaskResponse>> GetTask(string id) {
            if (id == null)
                return BadRequest("No task identifier provided to retrieve");
            
            Entities.Task task = await _dbContext.Tasks.FindAsync(id);
            if (task == null)
                return NotFound(new ErrorResponse("TASK_NOT_FOUND", "Task not found"));

            Guid? principalId = User.GetUserId();
            if (!User.IsAdmin() && task.CreatedById != principalId)
                return StatusCode(403, new ErrorResponse("TASK_FORBIDDEN", "You cannot access this resource"));

            return Ok(TaskMapper.ToTaskResponse(task));
        }

        [HttpDelete("{id}", Name = "DeleteTask")]
        [Authorize]
        public async Task<ActionResult> DeleteTask(string id) {
            if (id == null)
                return BadRequest("No task identifier provided to delete");

            Entities.Task task = await _dbContext.Tasks.FindAsync(id);
            if (task == null) return NoContent();

            Guid? principalId = User.GetUserId();
            if (!User.IsAdmin() && !principalId.Equals(task.CreatedById))
                return StatusCode(403, new ErrorResponse("TASK_FORBIDDEN", "You cannot access this resource"));
            
            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

    }
}
