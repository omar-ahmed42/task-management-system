namespace backend.Dtos.Tasks;

public class TaskResponse {
    public string Id{get; set;}
    public string Title{get; set;}
    public string Description{get; set;}
    public DateTime DueDate{get; set;}
    public Enums.TaskStatus Status{get; set;}
    public int PriorityLevel{get; set;}

    public TaskResponse() {}
    public TaskResponse(string id, string title, string description, DateTime dueDate, Enums.TaskStatus status, int priorityLevel) {
        Id = id;
        Title = title;
        Description = description;
        DueDate = dueDate;
        Status = status;
        PriorityLevel = priorityLevel;
    }
}
