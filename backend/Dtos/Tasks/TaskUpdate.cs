using Microsoft.AspNetCore.Mvc;

namespace backend.Dtos.Tasks;

public class TaskUpdate
{
    [FromRoute(Name = "id")]
    public string Id { get; set; }

    [FromBody]
    public TaskUpdateDetails Task { get; set; }
}
