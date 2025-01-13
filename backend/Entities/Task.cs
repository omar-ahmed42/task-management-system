using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities;

[Table(name: "tasks")]
public class Task
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [Column("id")]
    public string Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("title")]
    public string Title { get; set; }

    [Required]
    [MaxLength(32768)]
    [Column("description")]
    public string Description { get; set; }

    [Column("due_date")]
    public DateTime DueDate { get; set; }

    [Required]
    [EnumDataType(typeof(TaskStatus))]
    [Column("status")]
    public Enums.TaskStatus Status { get; set; }

    [Required]
    [Column("priority_level")]
    public int PriorityLevel { get; set; }

    [Required]
    [Column("created_by_id")]
    public Guid CreatedById { get; set; }

    [ForeignKey("CreatedById")]
    public User CreatedBy { get; set; }
}
