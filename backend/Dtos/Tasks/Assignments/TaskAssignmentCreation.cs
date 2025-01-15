using Microsoft.AspNetCore.Mvc;

namespace backend.Dtos.Tasks.Assignments;

public class TaskAssignmentCreation
{

    [FromRoute(Name = "task-id")] 
    public string TaskId {get;set;}

    [FromBody]
    public List<TaskAssigneesCreation> Assignees {get;set;}

}
