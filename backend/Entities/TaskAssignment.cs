using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Entities;

[Table("tasks_assignments")]
[PrimaryKey(nameof(TaskId), nameof(AssigneeId), nameof(AssigneeType))]
public class TaskAssignment
{
    [Column("task_id")]
    public required string TaskId{ get; set; }

    [Column("assignee_id")]
    public required Guid AssigneeId { get; set; }

    [Column("assignee_type")]
    [Unicode(false)]
    [MaxLength(15)]
    public required AssigneeType AssigneeType { get; set; }

    [ForeignKey("TaskId")]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Entities.Task Task{ get; set; }

    [Column("assigned_at")]
    public DateTime AssignedAt{ get; set; }
}
