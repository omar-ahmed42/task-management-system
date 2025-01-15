namespace backend.Dtos.Tasks.Assignments;

public record class TaskUserAssigneeDetails(string TaskId, Guid Id, string FirstName, string LastName, string Email, DateTime AssignedAt);