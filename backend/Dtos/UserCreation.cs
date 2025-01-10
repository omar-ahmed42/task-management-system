using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

public record UserCreation([Required(AllowEmptyStrings = false)][MaxLength(50)] string FirstName, [Required(AllowEmptyStrings = false)] [MaxLength(50)] string LastName, [EmailAddress] string Email, string Password, [Required] DateOnly BirthDate, [Required(ErrorMessage = "Role cannot be empty")][MinLength(32, ErrorMessage = "MIL32: Invalid role")][MaxLength(36, ErrorMessage = "MAL36: Invalid Role")] string roleId);
