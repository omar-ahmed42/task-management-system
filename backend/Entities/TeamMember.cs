using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace backend.Entities;

[Table("teams_members")]
[PrimaryKey(nameof(TeamId), nameof(MemberId))]
public class TeamMember
{

    [Required]
    [Column("team_id")]
    public required string TeamId { get; set; }

    [Required]
    [Column("member_id")]
    public required Guid MemberId { get; set; }

    [ForeignKey("TeamId")]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Team Team { get; set; }

    [ForeignKey("MemberId")]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public User Member { get; set; }

}
