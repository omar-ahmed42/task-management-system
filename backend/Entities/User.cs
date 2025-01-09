using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Entities;

public class User : IdentityUser<Guid>
{

    [Required][Column(name:"first_name")][Unicode(false)][MinLength(1)][MaxLength(50)]
    public String FirstName{ get; set; }

    [Required][Column(name:"last_name")][Unicode(false)][MinLength(1)][MaxLength(50)]
    public String LastName{ get; set; }

    [Required][Column("birth_date")]
    public DateOnly BirthDate{ get; set; }

    public void SetUsername(string email) {
        UserName = email;
        Email = email;
    }
}
