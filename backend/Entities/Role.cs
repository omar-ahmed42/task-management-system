using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Entities;

public class Role : IdentityRole<Guid>
{
    [Unicode(false)][MaxLength(150)][Column("description")]
    public string Description { get; set; }

}
