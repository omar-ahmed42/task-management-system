using backend.Enums;

namespace backend.Dtos.Tasks.Assignments;

public class TaskAssigneesCreation
{

    public Guid AssigneeId { get; set; }
    public AssigneeType AssigneeType{ get; set; }

}
