namespace backend.Dtos.Tasks.Assignments;

public record class TaskTeamAssigneeDetails(string TaskId, Guid Id, string Name, string Description, Guid? LeaderId);