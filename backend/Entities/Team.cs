using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities;

[Table("teams")]
public class Team
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public string Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [MaxLength(250)]
    [Column("description")]
    public string Description { get; set; }

    [Column("leader_id")]
    public Guid? LeaderId { get; set; }

    [ForeignKey("LeaderId")]
    public User? Leader { get; set; }

    [Required]
    [Column("created_by_id")]
    public Guid CreatedById { get; set; }

    [ForeignKey("CreatedById")]
    
    public User? CreatedBy { get; set; }
}
